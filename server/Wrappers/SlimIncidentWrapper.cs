﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using Microsoft.Xrm.Sdk;

namespace MattermostCrmService.Wrappers
{

    public class SlimIncidentWrapper
    {
        private string m_networkAttachmentsFolder;

        public SlimIncidentWrapper(Entity incident, CrmWrapper wrapper)
        {

            Id = incident.Id;
            Url = "https://energynavigator.crm.dynamics.com/main.aspx?etc=112&id=" + incident.Id + "&histKey=1&newWindow=true&pagetype=entityrecord#392649339";
            Title = incident.GetAttributeValue<string>("title");
            Company = wrapper.LookupAccount(incident.GetAttributeValue<EntityReference>("customerid"));
            Owner = wrapper.LookupUser(incident.GetAttributeValue<EntityReference>("owninguser"));
            Creator = incident.GetAttributeValue<EntityReference>("createdby").Name;
            Description = incident.GetAttributeValue<string>("description");
            Version = incident.GetAttributeValue<string>("eni_version");
            CreatedOn = incident.GetAttributeValue<DateTime>("createdon");
            NetworkAttachmentsFolder = incident.GetAttributeValue<string>("eni_caseattachments").Trim();
            TicketNumber = incident.GetAttributeValue<string>("ticketnumber");

            //TODO: Fill this in
            switch ((incident.GetAttributeValue<OptionSetValue>("statuscode").Value))
            {
                case 1:
                    Status = "In-progress";
                    break;
                case 5:
                    Status = "Problem Solved";
                    break;
                case 1000:
                    Status = "Information Provided";
                    break;
                default:
                    Status = "Unknown Status - " + (incident.Attributes["statuscode"] as OptionSetValue).Value;
                    break;
            }

        }

        public Guid Id { get; set; }
        public string Url { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string TicketNumber { get; set; }
        public string Creator { get; set; }
        public string Status { get; set; }
        public string Owner { get; set; }
        public DateTime CreatedOn { get; set; }
        public string Company { get; set; }
        public string Version { get; set; }

        public string NetworkAttachmentsFolder
        {
            get { return m_networkAttachmentsFolder; }
            set { m_networkAttachmentsFolder = value?.Trim(); }
        }
    }

}
