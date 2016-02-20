using JsonConfig;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using System;
using System.Collections.Generic;
using System.ServiceModel.Description;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Timers;
using System.IO;

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
            CrmWrapper.Init(Config.MergedConfig.CrmUser, password != null ? password : Config.MergedConfig.CrmPassword, Config.MergedConfig.CrmUrl);

            var listeningOn = Config.MergedConfig.Listen;
            var appHost = new AppHost()
                .Init()
                .Start(listeningOn);

            Console.WriteLine("AppHost Created at {0}, listening on {1}", DateTime.Now, listeningOn);

            Console.ReadKey();
        }

        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine("Reconnect");
            CrmWrapper.Instance.Reconnect();
        }
    }
}
