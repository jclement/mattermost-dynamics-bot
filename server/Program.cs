﻿using JsonConfig;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.ServiceModel.Description;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Timers;

namespace server
{

    [Route("/version")]
    public class Version
    {
    }

    [Route("/incident/{CaseNum}")]
    public class Incident
    {
        public string CaseNum { get; set; }
    }

    [Route("/receivemessage")]
    public class MatterMostMessage
    {
        public string text { get; set; }
        public string user_name { get; set; }
    }

    public class MatterMostResponse
    {
        public string text { get; set; }
        public string username { get; set; }
        public string icon_url { get; set; }
    }

    public class IncidentMarkdowner
    {
        public static string ConvertToMarkdown(IncidentWrapper incident, string caseNumber)
        {
            StringBuilder sb = new StringBuilder();
            if (incident != null)
            {
                sb.AppendLine($"__Case__ : [{caseNumber}]({Config.MergedConfig.WebRoot}/static/#/incident/{caseNumber})");
                sb.AppendLine($"__Title__ : {incident.Title}");
                sb.AppendLine($"__Company__ : {incident.Company}");
                sb.AppendLine($"__Owner__ : {incident.Owner}");
                sb.AppendLine($"__Description__ : {incident.Description}");
            } else
            {
                sb.AppendLine($"__Case__ : {caseNumber}");
                sb.AppendLine("__NOT FOUND__");
            }
            return sb.ToString();
        }
    }

    public class CRMService : Service
    {
        private static Regex CASRegex = new Regex("\\b(CAS-[0-9]{5}-[A-Z][0-9][A-Z][0-9][A-Z][0-9])\\b");

        public object Any(Incident request)
        {
            return CrmWrapper.Instance.GetIncident(request.CaseNum);
        }

        public object Get(Version request)
        {
            return CrmWrapper.Instance.Version;
        }

        public object Post(MatterMostMessage request)
        {
            List<string> cases = new List<string>();
            HashSet<string> visitedCases = new HashSet<string>();
            foreach (Match match in CASRegex.Matches(request.text))
            {
                if (!visitedCases.Contains(match.Value))
                {
                    visitedCases.Add(match.Value);
                    IncidentWrapper result = CrmWrapper.Instance.GetIncident(match.Value);
                    cases.Add(IncidentMarkdowner.ConvertToMarkdown(result, match.Value));
                }
            }
            if (cases.Count > 0)
            {
                return new MatterMostResponse { text = string.Join(Environment.NewLine, cases), username = "CRM-Bot", icon_url = Config.MergedConfig.WebRoot + "/static/reaper.png" };
            }
            return null;
        }
    }

    public class AppHost : AppSelfHostBase
    {
        public AppHost() : base("HttpListener Self-Host", typeof (CRMService).Assembly)
        {
        }

        public override void Configure(Funq.Container container)
        {
            SetConfig(new HostConfig
            {
#if DEBUG
                DebugMode = true,
                WebHostPhysicalPath = "~/../..".MapServerPath(),
#endif
            });
        }
    }

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
                password = Console.ReadLine().Trim();
            }

            CrmWrapper.Init(Config.MergedConfig.CrmUser, password != null ? password : Config.MergedConfig.CrmPassword, Config.MergedConfig.CrmUrl);

            //var newNote = CrmWrapper.Instance.AddNote(Guid.Parse("5bd8cb2f-c0bf-e511-80f5-3863bb367d10"), "JSC TEST API", "World");

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
