using System;
using JsonConfig;
using Microsoft.Xrm.Sdk;

namespace MattermostCrmService.Wrappers
{

    public class SlimIncidentWrapper
    {
        private string m_networkAttachmentsFolder;

        public SlimIncidentWrapper(Entity incident, CrmWrapper wrapper)
        {
            Incident incidentObject = (Incident)incident;
            Id = incidentObject.Id;
            Url = "https://" + Config.MergedConfig.CrmOrg + ".crm.dynamics.com/main.aspx?etc=" + Incident.EntityTypeCode + "&id=" + incidentObject.Id + "&histKey=1&newWindow=true&pagetype=entityrecord#392649339";
            Title = incidentObject.Title;
            Company = wrapper.LookupAccount(incidentObject.CustomerId);
            Owner = wrapper.LookupUser(incidentObject.OwnerId);
            Creator = incidentObject.CreatedBy.Name;
            Description = incidentObject.Description;
            Version = incidentObject.eni_Version;
            CreatedOn = incidentObject.CreatedOn.Value;
            NetworkAttachmentsFolder = incidentObject.eni_CaseAttachments?.Trim();
            TicketNumber = incidentObject.TicketNumber;

            Status = MapStatusCodeToString(incidentObject.StatusCode);
            State = MapStateToString(incidentObject.StateCode);
        }

        public Guid Id { get; set; }
        public string Url { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string TicketNumber { get; set; }
        public string Creator { get; set; }
        public string Status { get; set; }
        public string State { get; set; }
        public string Owner { get; set; }
        public DateTime CreatedOn { get; set; }
        public string Company { get; set; }
        public string Version { get; set; }

        public string NetworkAttachmentsFolder
        {
            get { return m_networkAttachmentsFolder; }
            set { m_networkAttachmentsFolder = value?.Trim(); }
        }

        private string MapStateToString(IncidentState? state)
        {
            if (state == null) {
                return null;
            } else if (state == IncidentState.Active) {
                return "Active";
            }
            else if (state == IncidentState.Resolved) {
                return "Resolved";
            }
            else if (state == IncidentState.Canceled) {
                return "Canceled";
            }
            return "Unknown State - " + state;
        }

        private string MapStatusCodeToString(OptionSetValue value)
        {
            if (value.Value.Equals((int) Incident_StatusCode.InProgress))
            {
                return "In-progress";
            }
            else if (value.Value.Equals((int) Incident_StatusCode.Canceled))
            {
                return "Canceled";
            }
            else if (value.Value.Equals((int) Incident_StatusCode.BugCreated))
            {
                return "Bug Created";
            }
            else if (value.Value.Equals((int) Incident_StatusCode.ConversionProvided))
            {
                return "Conversion Provided";
            }
            else if (value.Value.Equals((int) Incident_StatusCode.ClarificationProvided))
            {
                return "Clarification Provided";
            }
            else if (value.Value.Equals((int) Incident_StatusCode.OnHold))
            {
                return "On Hold";
            }
            else if (value.Value.Equals((int) Incident_StatusCode.WaitingforDetails))
            {
                return "Waiting for Details";
            }
            else if (value.Value.Equals((int) Incident_StatusCode.Researching))
            {
                return "Researching";
            }
            else if (value.Value.Equals((int) Incident_StatusCode.ProblemSolved))
            {
                return "Problem Solved";
            }
            else if (value.Value.Equals((int) Incident_StatusCode.InformationProvided))
            {
                return "Information Provided";
            }
            else if (value.Value.Equals((int) Incident_StatusCode.CustomReportProvided))
            {
                return "Custom Report Provided";
            }
            else if (value.Value.Equals((int) Incident_StatusCode.EnchancementRequestCreated))
            {
                return "Enhancement Request Created";
            }
            else if (value.Value.Equals((int) Incident_StatusCode.GrantedWebsiteAccess))
            {
                return "Granted Website Access";
            }
            else if (value.Value.Equals((int) Incident_StatusCode.KnownIssue))
            {
                return "Known Issue";
            }
            else if (value.Value.Equals((int) Incident_StatusCode.LicenseActivated))
            {
                return "License Activated";
            }
            else if (value.Value.Equals((int) Incident_StatusCode.Merged))
            {
                return "Merged";
            }
            else if (value.Value.Equals((int) Incident_StatusCode.NewSolutionProvided))
            {
                return "New Solution Provided";
            }
            else if (value.Value.Equals((int) Incident_StatusCode.NoResponseFromClient))
            {
                return "No Response From Client";
            }
            else if (value.Value.Equals((int) Incident_StatusCode.NoSolutionFound))
            {
                return "No Solution Found";
            }
            else if (value.Value.Equals((int) Incident_StatusCode.PreviousSolutionProvided))
            {
                return "Previous Solution Provided";
            }
            else if (value.Value.Equals((int) Incident_StatusCode.ProvidedToolorPlugin))
            {
                return "Provided Tool or Plugin";
            }
            else if (value.Value.Equals((int) Incident_StatusCode.TrainingProvided))
            {
                return "Training Provided";
            }
            else if (value.Value.Equals((int) Incident_StatusCode.ThirdPartyInformationProvided_Royalties))
            {
                return "Third Party Information Provided - Royalties";
            }
            return "Unknown Status - " + value.Value;
        }
    }

}
