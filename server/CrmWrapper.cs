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
using Microsoft.Xrm.Sdk.Messages;
using ServiceStack.Text;

namespace server
{
    public class IncidentWrapper
    {
        public IncidentWrapper(Entity incident, CrmWrapper wrapper)
        {
            EntityReference userRef = incident.Attributes["owninguser"] as EntityReference;
            DataCollection<Entity> userCollection = wrapper.RunQuery("systemuser", new string[] { "fullname" }, "systemuserid", new string[] { userRef.Id.ToString("d") });
            EntityReference customerRef = incident.Attributes["customerid"] as EntityReference;
            DataCollection<Entity> accountCollection = wrapper.RunQuery("account", new string[] { "name" }, "accountid", new string[] { customerRef.Id.ToString("d") });

            Id = incident.Id.ToString();
            Url = "https://energynavigator.crm.dynamics.com/main.aspx?etc=112&id=" + incident.Id +
                  "&histKey=1&newWindow=true&pagetype=entityrecord#392649339";
            Title = incident.Attributes["title"] as String;
            Company = ((accountCollection.Count == 0) ? "Not Found" : accountCollection[0].Attributes["name"]) as String;
            Owner = ((userCollection.Count == 0) ? "Not Found" : userCollection[0].Attributes["fullname"]) as String;
            Description = incident.Attributes["description"] as String;
            Notes = wrapper.GetNotes(incident.Id).ToArray();
            CaseAttachments = incident.Attributes.ContainsKey("eni_caseattachments") ? incident.Attributes["eni_caseattachments"] as string : "";
        }

        public string Id { get; set; }
        public string Url { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Owner { get; set; }
        public string Company { get; set; }
        public string CaseAttachments { get; set; }
        public NoteWrapper[] Notes { get; set; }
        
    }

    public class NoteWrapper
    {
        public NoteWrapper(Entity annotation, CrmWrapper wrapper)
        {
            Body = annotation.Attributes.ContainsKey("notetext")? annotation.Attributes["notetext"] as String : "";
            Filename = annotation.Attributes.ContainsKey("filename")? annotation.Attributes["filename"] as String : "";
            Filesize = annotation.Attributes.ContainsKey("filesize")? (Int32?) annotation.Attributes["filesize"] : (Int32?) null;
            Mimetype = annotation.Attributes.ContainsKey("mimetype")? annotation.Attributes["mimetype"] as String: "";
            Title = annotation.Attributes.ContainsKey("subject")? annotation.Attributes["subject"] as String: "Mysterious Untitled Note";
            EntityReference userRef = annotation.Attributes["owninguser"] as EntityReference;
            DataCollection<Entity> userCollection = wrapper.RunQuery("systemuser", new string[] { "fullname" }, "systemuserid", new string[] { userRef.Id.ToString("d") });
            Owner = ((userCollection.Count == 0) ? "Not Found" : userCollection[0].Attributes["fullname"]) as String;
            Created = Convert.ToDateTime(annotation.Attributes["createdon"]);
            Modified = annotation.Attributes["modifiedon"] == null ? (DateTime?) null : Convert.ToDateTime(annotation.Attributes["modifiedon"]);
        }
        public string Title { get; set; }
        public string Body { get; set; }
        public string Owner { get; set; }
        public string Filename { get; set; }
        public Int32? Filesize { get; set; }
        public string Mimetype { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Modified { get; set; }
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

        public DataCollection<Entity> RunQuery(string entityName, string conditionFieldName, string[] conditionValues)
        {
            QueryExpression query = new QueryExpression()
            {
                EntityName = entityName,
                ColumnSet = new ColumnSet(true),
                Criteria = {
                        Conditions = {
                            new ConditionExpression (conditionFieldName, ConditionOperator.In, conditionValues)
                        }
                    }
            };

            return m_service.RetrieveMultiple(query).Entities;
        }

        public DataCollection<Entity> RunQuery(string entityName, string[] columns, string conditionFieldName, string[] conditionValues)
        {
            QueryExpression query = new QueryExpression()
            {
                EntityName = entityName,
                ColumnSet = new ColumnSet(columns),
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

        public IEnumerable<NoteWrapper> GetNotes(Guid incidentId)
        {
            var notes = new List<NoteWrapper>();
            DataCollection<Entity>  annotations = RunQuery("annotation", "objectid", new string[] {incidentId.ToString()});
            foreach (var annotation in annotations)
            {
                notes.Add(new NoteWrapper(annotation, this));
            }
            return notes;
        }

        public NoteWrapper AddNote(Guid incidentId, string title, string body)
        {
            // test code.  doesn't work!  Well it works but the note goes missing.  That's probably bad!
            Entity entity = new Entity("annotation");
            entity.Attributes.Add("objectid", incidentId.ToString());
            entity.Attributes.Add("subject", title);
            entity.Attributes.Add("notetext", body);
            var id = m_service.Create(entity);
            var newEntity = m_service.Retrieve("annotation", id, new ColumnSet(true));
            return new NoteWrapper(newEntity, this);
        }

        public IncidentWrapper GetIncident(string caseNum)
        {
            DataCollection<Entity> entityCollection = RunQuery("incident", "ticketnumber", new string[] { caseNum });

            DumpCollection(entityCollection);

            if (entityCollection.Count == 0)
            {
                //throw new ApplicationException("Not found");
                return null;
            }

            return new IncidentWrapper(entityCollection[0], this);
        }
    }
}
