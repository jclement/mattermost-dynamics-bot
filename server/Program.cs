﻿using JsonConfig;
using System;
using System.Net;
using System.Timers;
using ServiceStack;
using MattermostCrmService.Messages;

namespace MattermostCrmService
{
    class Program
    {
        static void Main(string[] args)
        {
            Timer timer = new Timer(1000 * 60 * 60 * 5);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();

            string password = null;
            if (string.IsNullOrEmpty(Config.MergedConfig.CrmPassword))
            {
                Console.Write($"Password for {Config.MergedConfig.CrmUser}:");
                Console.ForegroundColor = ConsoleColor.Black;
                password = Console.ReadLine().Trim();
                Console.ForegroundColor = ConsoleColor.White;
                Console.Clear();
            }

            LoginHelper.Init(Config.MergedConfig.CrmKey);
            CrmConnectionManager.Init(Config.MergedConfig.CrmUser, password ?? Config.MergedConfig.CrmPassword, "https://"+ Config.MergedConfig.CrmOrg +".crm.dynamics.com/XRMServices/2011/Organization.svc");

            var listeningOn = Config.MergedConfig.Listen;
            var appHost = new AppHost();
            appHost.Init();
            appHost.GlobalRequestFilters.Add((req, res, obj) =>
            {
                if (req.Dto is AuthenticatedRequestBase)
                {
                    var request = (AuthenticatedRequestBase) req.Dto;
                    if (string.IsNullOrEmpty(request.AuthenticationToken) && !string.IsNullOrEmpty(req.GetHeader("X-AUTH-TOKEN")))
                        request.AuthenticationToken = req.GetHeader("X-AUTH-TOKEN");
                    if (String.IsNullOrEmpty(request.AuthenticationToken))
                        throw new ApplicationException("No Auth Token");
                    var authInfo = LoginHelper.Instance.ParseToken(request.AuthenticationToken);
                    if (string.IsNullOrEmpty(authInfo.Username))
                        throw new ApplicationException("No Valid Auth Token");
                }
                if (req.Dto is MattermostRequestBase)
                {
                    var request = (MattermostRequestBase) req.Dto;
                    if (String.IsNullOrEmpty(request.token))
                        throw new ApplicationException("No Token");
                    foreach (var token in Config.MergedConfig.WebhookTokens)
                    {
                        if (request.token.Equals(token))
                            return;
                    }
                    throw new ApplicationException("Not a valid token");
                }
            });
            appHost.Start(listeningOn);


            Console.WriteLine("AppHost Created at {0}, listening on {1}", DateTime.Now, listeningOn);

            Console.ReadKey();
        }

        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine("Reconnect");
            CrmConnectionManager.Instance.ReconnectAll();
        }
    }
}
