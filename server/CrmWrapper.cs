﻿using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Description;
using System.IO;
using System.Runtime.Serialization;
using System.Security.AccessControl;
using ServiceStack.DataAnnotations;
using System.Security.Principal;

namespace server
{
    public class NetworkAttachmentWrapper
    {
        public string Filename { get; set; }
        public string Owner { get; set; }
        public string Path { get; set; }
        public DateTime Modified { get; set; }
        public DateTime Created { get; set; }
        public long SizeKB { get; set; }
    }

    public class IncidentWrapper
    {
        public IncidentWrapper(Entity incident, CrmWrapper wrapper)
        {
            EntityReference userRef = incident.Attributes["owninguser"] as EntityReference;
            DataCollection<Entity> userCollection = wrapper.RunQuery("systemuser", new string[] { "fullname" }, "systemuserid", new string[] { userRef.Id.ToString("d") });
            EntityReference customerRef = incident.Attributes["customerid"] as EntityReference;
            DataCollection<Entity> accountCollection = wrapper.RunQuery("account", new string[] { "name" }, "accountid", new string[] { customerRef.Id.ToString("d") });

            Id = incident.Id;
            Url = "https://energynavigator.crm.dynamics.com/main.aspx?etc=112&id=" + incident.Id +
                  "&histKey=1&newWindow=true&pagetype=entityrecord#392649339";
            Title = incident.Attributes["title"] as String;
            Company = ((accountCollection.Count == 0) ? "Not Found" : accountCollection[0].Attributes["name"]) as String;
            Owner = ((userCollection.Count == 0) ? "Not Found" : userCollection[0].Attributes["fullname"]) as String;
            Description = incident.Attributes["description"] as String;
            Version = incident.Attributes.ContainsKey("eni_version")? incident.Attributes["eni_version"] as String: "";
            Notes = wrapper.GetNotes(incident.Id).ToArray();
            NetworkAttachmentsFolder = incident.Attributes.ContainsKey("eni_caseattachments") ? incident.Attributes["eni_caseattachments"] as string : "";
            Status = (incident.Attributes["statuscode"] as OptionSetValue).Value.ToString();

            switch ((incident.Attributes["statuscode"] as OptionSetValue).Value)
            {
                case 1:
                    Status = "In-progress";
                    break;
                default:
                    Status = "Unknown Status - " + (incident.Attributes["statuscode"] as OptionSetValue).Value;
                    break;
            }

            Creator = (incident.Attributes["createdby"] as EntityReference).Name;

            var attachments = new List<NetworkAttachmentWrapper>();
            if (!string.IsNullOrEmpty(NetworkAttachmentsFolder) && Directory.Exists(NetworkAttachmentsFolder))
            {
                foreach (var path in Directory.GetFiles(NetworkAttachmentsFolder))
                {
                    var filename = Path.GetFileName(path);
                    var info = new FileInfo(path);
                    var sec = System.IO.File.GetAccessControl(path);
                    attachments.Add(new NetworkAttachmentWrapper()
                    {
                        Filename = filename,
                        Path = path,
                        Created = info.CreationTime,
                        Modified = info.LastWriteTime,
                        SizeKB = info.Length / 1024,
                        Owner = sec.GetOwner(typeof(SecurityIdentifier)).Translate(typeof(NTAccount)).Value
                    });
                }
            }
            NetworkAttachments = attachments.ToArray();
        }

        public Guid Id { get; set; }
        public string Url { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Creator { get; set; }
        public string Status { get; set; }
        public string Owner { get; set; }
        public string Company { get; set; }
        public string Version { get; set; }
        public NoteWrapper[] Notes { get; set; }
        public string NetworkAttachmentsFolder { get; set; }
        public NetworkAttachmentWrapper[] NetworkAttachments { get; set; }
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
            EntityReference ownerRef = annotation.Attributes["owninguser"] as EntityReference;
            DataCollection<Entity> ownerCollection = wrapper.RunQuery("systemuser", new string[] { "fullname" }, "systemuserid", new string[] { ownerRef.Id.ToString("d") });
            Owner = ((ownerCollection.Count == 0) ? "Not Found" : ownerCollection[0].Attributes["fullname"]) as String;
            Created = Convert.ToDateTime(annotation.Attributes["createdon"]);
            Modified = annotation.Attributes["modifiedon"] == null ? (DateTime?) null : Convert.ToDateTime(annotation.Attributes["modifiedon"]);
            Id = annotation.Id.ToString("d");
        }
        public string Title { get; set; }
        public string Body { get; set; }
        public string Owner { get; set; }
        public string Filename { get; set; }
        public Int32? Filesize { get; set; }
        public string Mimetype { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Modified { get; set; }
        public string Id { get; set; }
    }

    public class AttachmentFileWrapper
    {
        public Stream FileData { get; set; }
        public string MimeType { get; set; }
    }

    public class CrmWrapper
    {
        private static CrmWrapper m_instance;
        private IOrganizationService m_service;

        private string m_user;
        private string m_password;
        private string m_url;

        private CrmWrapper(string username, string password, string url)
        {
            m_user = username;
            m_password = password;
            m_url = url;
            Connect();
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
            entity.Attributes.Add("objectid", new EntityReference("incident", incidentId));
            entity.Attributes.Add("subject", title);
            entity.Attributes.Add("notetext", body + "\n\n(Comment posted by the unCRM and not necessarily by Jeff)");
            entity.Attributes.Add("objecttypecode", 112);
            var id = m_service.Create(entity);
            var newEntity = m_service.Retrieve("annotation", id, new ColumnSet(true));

            return new NoteWrapper(newEntity, this);
        }

        public AttachmentFileWrapper GetAttachmentFile(Guid attachmentId)
        {
            DataCollection<Entity> attachmentEntities = RunQuery("annotation", new string[] { "mimetype", "documentbody" }, "annotationid", new string[] { attachmentId.ToString("d") });
            
            if (attachmentEntities.Count == 0)
            {
                return null;
            }
            byte[] fileData = Convert.FromBase64String(attachmentEntities[0].Attributes["documentbody"].ToString());
            return new AttachmentFileWrapper { FileData = new MemoryStream(fileData), MimeType = attachmentEntities[0].Attributes["mimetype"] as string};
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
