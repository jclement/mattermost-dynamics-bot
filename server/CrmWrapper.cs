using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;

namespace server
{
    public class IncidentWrapper
    {
        public string Url { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Owner { get; set; }
        public string Company { get; set; }
        
    }
    public class CrmWrapper
    {
        private static CrmWrapper m_instance;
        private IOrganizationService m_service;

        public static void ConnectToMSCRM(string UserName, string Password, string SoapOrgServiceUri)
        {
        }


        private CrmWrapper(string username, string password, string url)
        {
            ClientCredentials credentials = new ClientCredentials();
            credentials.UserName.UserName = username;
            credentials.UserName.Password = password;
            Uri serviceUri = new Uri(url);
            OrganizationServiceProxy proxy = new OrganizationServiceProxy(serviceUri, null, credentials, null);
            proxy.EnableProxyTypes();
            m_service = (IOrganizationService)proxy;
        }

        public static void Init(string username, string password, string url)
        {
            m_instance = new CrmWrapper(username, password, url);
        }

        public static CrmWrapper Instance { get { return m_instance; } }

        public string Version
        {
            get
            {
                RetrieveVersionRequest versionRequest = new RetrieveVersionRequest();
                RetrieveVersionResponse versionResponse = (RetrieveVersionResponse) m_service.Execute(versionRequest);
                return versionResponse.Version;
            }
        }

        private DataCollection<Entity> RunQuery(string entityName, string[] columns, string conditionFieldName, string[] conditionValues)
        {
            QueryExpression query = new QueryExpression()
            {
                EntityName = entityName,
                ColumnSet = new ColumnSet(columns),
                TopCount = 1,
                Criteria = {
                        Conditions = {
                            new ConditionExpression (conditionFieldName, ConditionOperator.In, conditionValues)
                        }
                    }
            };

            return m_service.RetrieveMultiple(query).Entities;
        }

        private void DumpCollection(DataCollection<Entity> entityCollection)
        {
            foreach (var entity in entityCollection)
            {
                Console.WriteLine("====================================");
                foreach (var attribute in entity.Attributes)
                {
                    Console.WriteLine(attribute.Key + ": " + attribute.Value);
                }
            }
        }

        public IncidentWrapper GetIncident(string caseNum)
        {
            DataCollection<Entity> entityCollection = RunQuery("incident", new string[] { "incidentid", "description", "title", "ticketnumber", "customerid", "owninguser" }, "ticketnumber", new string[] { caseNum });

            if (entityCollection.Count == 0)
            {
                //throw new ApplicationException("Not found");
                return null;
            }

            EntityReference userRef = entityCollection[0].Attributes["owninguser"] as EntityReference;
            DataCollection<Entity> userCollection = RunQuery("systemuser", new string[] { "fullname" }, "systemuserid", new string[] { userRef.Id.ToString("d") });
            EntityReference customerRef = entityCollection[0].Attributes["customerid"] as EntityReference;
            DataCollection<Entity> accountCollection = RunQuery("account", new string[] { "name" }, "accountid", new string[] { customerRef.Id.ToString("d") });

            return new IncidentWrapper()
            {
                Url = "https://energynavigator.crm.dynamics.com/main.aspx?etc=112&id=" + entityCollection[0].Id + "&histKey=1&newWindow=true&pagetype=entityrecord#392649339",
                Title = entityCollection[0].Attributes["title"] as String,
                Company = ((accountCollection.Count == 0) ? "Not Found" : accountCollection[0].Attributes["name"]) as String,
                Owner = ((userCollection.Count == 0) ? "Not Found" : userCollection[0].Attributes["fullname"]) as String,
                Description = entityCollection[0].Attributes["description"] as String
            };
        }
    }
}
