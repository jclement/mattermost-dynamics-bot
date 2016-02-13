using JsonConfig;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using ServiceStack;
using System;
using System.ServiceModel.Description;

namespace server
{

    [Route("/version")]
    public class Version
    {
    }

    [Route("/incident/{id}")]
    public class Incident
    {
        public Guid Id { get; set; }
    }

    public class IncidentResponse
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Owner { get; set; }
    }

    public class CRMService : Service
    {
        public object Any(Incident request)
        {
            return new IncidentResponse { Title = request.Id.ToString() };
        }

        public object Get(Version request)
        {
            return CrmWrapper.Instance.Version;
        }
    }

    public class AppHost : AppSelfHostBase
    {
        public AppHost() : base("HttpListener Self-Host", typeof (CRMService).Assembly)
        {
        }

        public override void Configure(Funq.Container container) { }
    }

    class Program
    {
        static void Main(string[] args)
        {
            CrmWrapper.Init(Config.MergedConfig.CrmUser, Config.MergedConfig.CrmPassword, Config.MergedConfig.CrmUrl);

            var listeningOn = Config.MergedConfig.Listen;
            var appHost = new AppHost()
                .Init()
                .Start(listeningOn);

            Console.WriteLine("AppHost Created at {0}, listening on {1}", DateTime.Now, listeningOn);

            Console.ReadKey();
        }
    }
}
