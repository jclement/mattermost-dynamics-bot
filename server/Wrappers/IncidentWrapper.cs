using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using Microsoft.Xrm.Sdk;

namespace MattermostCrmService.Wrappers
{

    public class IncidentWrapper
    {
        public IncidentWrapper(Entity incident, CrmWrapper wrapper)
        {

            Id = incident.Id;
            Url = "https://energynavigator.crm.dynamics.com/main.aspx?etc=112&id=" + incident.Id + "&histKey=1&newWindow=true&pagetype=entityrecord#392649339";
            Title = incident.GetAttributeValue<string>("title");
            Company = wrapper.LookupAccount(incident.GetAttributeValue<EntityReference>("customerid"));
            Owner = wrapper.LookupUser(incident.GetAttributeValue<EntityReference>("owninguser"));
            Creator = incident.GetAttributeValue<EntityReference>("createdby").Name;
            Description = incident.GetAttributeValue<string>("description");
            Version = incident.GetAttributeValue<string>("eni_version");
            Notes = wrapper.GetNotes(incident.Id).ToArray();
            NetworkAttachmentsFolder = incident.GetAttributeValue<string>("eni_caseattachments");

            //TODO: Fill this in
            switch ((incident.GetAttributeValue<OptionSetValue>("statuscode").Value))
            {
                case 1:
                    Status = "In-progress";
                    break;
                default:
                    Status = "Unknown Status - " + (incident.Attributes["statuscode"] as OptionSetValue).Value;
                    break;
            }

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

}
