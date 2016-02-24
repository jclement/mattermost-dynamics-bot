using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ServiceModel.Description;
using System.IO;
using System.Linq;
using MattermostCrmService.Wrappers;

namespace MattermostCrmService
{
    public class CrmWrapper
    {
        private static CrmWrapper m_instance;
        private static IDictionary<string, Guid> m_userLookup = new ConcurrentDictionary<string, Guid>(); 
        private static IDictionary<Guid, string> m_userNameLookup = new ConcurrentDictionary<Guid, string>(); 
        private static IDictionary<Guid, string> m_accounts = new ConcurrentDictionary<Guid, string>(); 
        private IOrganizationService m_service;

        private string m_user;
        private string m_password;
        private string m_url;

        public CrmWrapper(string username, string password, string url)
        {
            m_user = username;
            m_password = password;
            m_url = url;
            Connect();
        }

        private static void RefreshUserCache()
        {
            var userEntities = m_instance.RunQuery(SystemUser.EntityLogicalName);
            foreach (SystemUser e in userEntities)
            {
                m_userNameLookup.Add(Guid.Parse(e.SystemUserId.ToString()), e.FullName);
                m_userLookup.Add(e.FullName, Guid.Parse(e.SystemUserId.ToString()));
            }
        }

        public string LookupAccount(EntityReference user)
        {
            return LookupAccount(user.Id);
        }

        public string LookupAccount(Guid id)
        {
            string fullname;
            if (m_accounts.TryGetValue(id, out fullname))
            {
                return fullname;
            }

            DataCollection<Entity> collection = RunQuery(Account.EntityLogicalName, new string[] { "name" }, "accountid", new string[] { id.ToString("d") });

            if (collection.Count == 0)
            {
                m_accounts.Add(id, "NOT FOUND");
            }
            else
            {
                try
                {
                    m_accounts.Add(id, ((Account)collection[0]).Name);
                }
                catch 
                {
                }
            }
            return m_accounts[id];
        }

        public string LookupUser(EntityReference user)
        {
            return LookupUser(user.Id);
        }

        public string LookupUser(Guid id)
        {
            string fullname;
            if (m_userNameLookup.TryGetValue(id, out fullname))
            {
                return fullname;
            }
            return "NOT FOUND";
        }

        public IEnumerable<Tuple<string, Guid>> MatchUsersByName(string partialName)
        {
            return m_userLookup.Keys.Where(x => string.IsNullOrEmpty(partialName) || x.ToUpperInvariant().Contains(partialName.ToUpperInvariant()))
            .Select(x => new Tuple<string, Guid>(x, m_userLookup[x]));
        }

        private void Connect()
        {
            ClientCredentials credentials = new ClientCredentials();
            credentials.UserName.UserName = m_user;
            credentials.UserName.Password = m_password;
            Uri serviceUri = new Uri(m_url);
            OrganizationServiceProxy proxy = new OrganizationServiceProxy(serviceUri, null, credentials, null);
            proxy.EnableProxyTypes();
            m_service = (IOrganizationService)proxy;
            Guid userid = ((WhoAmIResponse)m_service.Execute(new WhoAmIRequest())).UserId;
            if (userid != Guid.Empty)
            {
                Console.WriteLine(userid);
            }
        }

        public static void Init(string username, string password, string url)
        {
            m_instance = new CrmWrapper(username, password, url);
            RefreshUserCache();
        }

        public void Reconnect()
        {
            Connect();
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

        public DataCollection<Entity> RunQuery(string entityName)
        {
            QueryExpression query = new QueryExpression
            {
                EntityName = entityName,
                ColumnSet = new ColumnSet(true),
                TopCount = 200
            };
            return m_service.RetrieveMultiple(query).Entities;
        }

        public DataCollection<Entity> RunQueryContains(string entityname, string conditionFieldName, string[] conditionValues)
        {
            QueryExpression query = new QueryExpression
            {
                EntityName = entityname,
                ColumnSet = new ColumnSet(true),
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression(conditionFieldName, ConditionOperator.Like, conditionValues)
                    }
                }
            };
            return m_service.RetrieveMultiple(query).Entities;
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
                    if (attribute.Value is OptionSetValue)
                    {
                        Console.WriteLine(attribute.Key + ": " + ((OptionSetValue) attribute.Value).Value);
                    }
                    else
                    {
                        Console.WriteLine(attribute.Key + ": " + attribute.Value);
                    }
                }
            }
        }

        public IEnumerable<SlimIncidentWrapper> SearchIncidents(string query, Guid? ownerId, Int32? stateCode)
        {
            QueryExpression queryExpression = new QueryExpression()
            {
                EntityName = Incident.EntityLogicalName,
                ColumnSet = new ColumnSet(true),
                TopCount = 200,
                Orders =
                {
                    new OrderExpression("createdon", OrderType.Descending)
                }
            };

            var le = queryExpression.AddLink(Account.EntityLogicalName, "customerid", "accountid");
            le.EntityAlias = "customer";
            le.Columns.AddColumn("name");
            le.JoinOperator = JoinOperator.Inner;

            FilterExpression productExpression = new FilterExpression(LogicalOperator.And);
            productExpression.Conditions.Add(new ConditionExpression("eni_product", ConditionOperator.Equal, (int) esi_product.AFENav));

            queryExpression.Criteria.FilterOperator = LogicalOperator.And;
            queryExpression.Criteria.AddFilter(productExpression);

            foreach (var token in query.tokens())
            {
                FilterExpression searchExpression = new FilterExpression(LogicalOperator.Or);
                searchExpression.Conditions.Add(new ConditionExpression("ticketnumber", ConditionOperator.Like,
                    "%" + token + "%"));
                searchExpression.Conditions.Add(new ConditionExpression("title", ConditionOperator.Like,
                    "%" + token + "%"));
                searchExpression.Conditions.Add(new ConditionExpression("description", ConditionOperator.Like,
                    "%" + token + "%"));
                searchExpression.Conditions.Add(new ConditionExpression("customer","name",ConditionOperator.Like, "%" + token + "%"));
                queryExpression.Criteria.AddFilter(searchExpression);
            }

            if (ownerId.HasValue)
            {
                FilterExpression userExpression = new FilterExpression(LogicalOperator.And);
                userExpression.Conditions.Add(new ConditionExpression("owninguser", ConditionOperator.Equal, ownerId));
                queryExpression.Criteria.AddFilter(userExpression);
            }
            

            if (stateCode.HasValue)
            {
                return m_service.RetrieveMultiple(queryExpression)
                    .Entities
                    .Where(x => stateCode.Value.Equals( ((Incident)x).StatusCode.Value ))
                    .Select(x => new SlimIncidentWrapper(x, this));
            }

            var res = m_service.RetrieveMultiple(queryExpression);
            return res
                .Entities
                .Select(x => new SlimIncidentWrapper(x, this));
        }

        public SlimIncidentWrapper UpdateOwner(Guid newOwnerId, Guid incidentId)
        {
            Incident incidentEntity = (Incident) m_service.Retrieve(Incident.EntityLogicalName, incidentId, new ColumnSet(true));
            incidentEntity.OwnerId = new EntityReference(SystemUser.EntityLogicalName, newOwnerId);
            m_service.Update(incidentEntity);
            return new SlimIncidentWrapper(incidentEntity, this);
        }

        public SlimIncidentWrapper UpdateNetworkAttachmentsFolder(string newPath, Guid incidentId)
        {
            Incident incidentEntity = (Incident) m_service.Retrieve(Incident.EntityLogicalName, incidentId, new ColumnSet(true));
            incidentEntity.eni_CaseAttachments = newPath?.Trim();
            m_service.Update(incidentEntity);
            return new SlimIncidentWrapper(incidentEntity, this);
        }


        public IEnumerable<NoteWrapper> GetNotes(Guid incidentId)
        {
            var notes = new List<NoteWrapper>();
            DataCollection<Entity>  annotations = RunQuery(Annotation.EntityLogicalName, "objectid", new string[] {incidentId.ToString()});
            foreach (var annotation in annotations)
            {
                notes.Add(new NoteWrapper(annotation, this));
            }
            return notes;
        }

        public void DeleteNote(Guid noteId)
        {
            m_service.Delete(Annotation.EntityLogicalName, noteId);
        }

        public NoteWrapper UpdateNote(Guid noteId, string title, string body)
        {
            Annotation note = (Annotation)m_service.Retrieve(Annotation.EntityLogicalName, noteId, new ColumnSet(true));
            note.Subject = title;
            note.NoteText = body;
            m_service.Update(note);
            return new NoteWrapper(note, this);
        }

        public NoteWrapper AddNote(Guid incidentId, string title, string body)
        {
            Annotation note = new Annotation();
            note.ObjectId = new EntityReference(Incident.EntityLogicalName, incidentId);
            note.Subject = title;
            note.NoteText = body;
            note.ObjectTypeCode = Incident.EntityTypeCode.ToString();
            var id = m_service.Create(note);
            var newEntity = m_service.Retrieve(Annotation.EntityLogicalName, id, new ColumnSet(true));

            return new NoteWrapper(newEntity, this);
        }

        public AttachmentFileWrapper GetAttachmentFile(Guid attachmentId)
        {
            DataCollection<Entity> attachmentEntities = RunQuery(Annotation.EntityLogicalName, new string[] { "mimetype", "documentbody" }, "annotationid", new string[] { attachmentId.ToString("d") });
            
            if (attachmentEntities.Count == 0)
            {
                return null;
            }
            byte[] fileData = Convert.FromBase64String(((Annotation) attachmentEntities[0]).DocumentBody);
            return new AttachmentFileWrapper { FileData = new MemoryStream(fileData), MimeType = ((Annotation) attachmentEntities[0]).MimeType};
        }


        public SlimIncidentWrapper GetSlimIncident(string caseNum)
        {
            DataCollection<Entity> entityCollection = RunQuery(Incident.EntityLogicalName, "ticketnumber", new string[] { caseNum });

            if (entityCollection.Count == 0)
            {
                return null;
            }

            return new SlimIncidentWrapper(entityCollection[0], this);
        }

        public IncidentWrapper GetIncident(string caseNum)
        {
            DataCollection<Entity> entityCollection = RunQuery(Incident.EntityLogicalName, "ticketnumber", new string[] { caseNum });

            DumpCollection(entityCollection);

            if (entityCollection.Count == 0)
            {
                return null;
            }

            return new IncidentWrapper(entityCollection[0], this);
        }
    }
}
