using JsonConfig;
using System;
using System.Net;
using System.Text;
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
                Console.Write($"Password for {Config.MergedConfig.CrmUser}: ");
                password = GetPasswordFromConsole();
                Console.Clear();
            }

            LoginHelper.Init(Config.MergedConfig.CrmKey ?? SecureRandomString.Generate(50));
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
                    if (authInfo == null) 
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

        private static string GetPasswordFromConsole()
        {
            StringBuilder password = new StringBuilder();
            bool readingPassword = true;

            while (readingPassword)
            {
                ConsoleKeyInfo userInput = Console.ReadKey(true);

                switch (userInput.Key)
                {
                    case (ConsoleKey.Enter):
                        readingPassword = false;
                        break;
                    case (ConsoleKey.Backspace):
                        if (password.Length > 0)
                        {
                            password.Remove(password.Length - 1, 1);
                            Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                            Console.Write(" ");
                            Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                        }
                        break;
                    default:
                        if (userInput.KeyChar != 0)
                        {
                            password.Append(userInput.KeyChar);
                            Console.Write("*");
                        }
                        break;
                }
            }
            Console.WriteLine();

            return password.ToString();
        }
    }
}
