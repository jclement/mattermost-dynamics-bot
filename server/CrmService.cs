using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using JsonConfig;
using ServiceStack;
using MattermostCrmService.Wrappers;
using MattermostCrmService.Messages;

namespace MattermostCrmService
{
    public class CRMService : Service
    {
        private static Regex CASRegex = new Regex("\\b(CAS-[0-9]{5}-[A-Z][0-9][A-Z][0-9][A-Z][0-9])\\b");

        private CrmWrapper GetAuthenticatedCrmWrapper(AuthenticatedRequestBase request)
        {
            if (String.IsNullOrEmpty(request.AuthenticationToken))
                throw new ApplicationException("No Auth Token");
            var authInfo = LoginHelper.Instance.ParseToken(request.AuthenticationToken);
            return new CrmWrapper(authInfo.Username, authInfo.Password, Config.MergedConfig.CrmUrl); 
        }

        public IncidentWrapper Any(Incident request)
        {
            return CrmWrapper.Instance.GetIncident(request.CaseNum);
        }

        public string Post(Login request)
        {
            try
            {
                new CrmWrapper(request.Username, request.Password, Config.MergedConfig.CrmUrl);
                return LoginHelper.Instance.GenerateToken(request.Username, request.Password);
            }
            catch
            {
                throw new ApplicationException("Authentication Failure");
            }
        }

        public SlimIncidentWrapper[] Get(BasicIncidentQuery request)
        {
            return CrmWrapper.Instance.SearchIncidents(request.Query).ToArray();
        }

        public object Get(Users request)
        {
            return CrmWrapper.Instance.MatchUsersByName(request.Query);
        }

        /*
        public object Get(Version request)
        {
            return CrmWrapper.Instance.Version;
        }
        */

        public NoteWrapper Post(UpdateNote request)
        {
            var crm = GetAuthenticatedCrmWrapper(request);
            return crm.UpdateNote(request.NoteId, request.Title, request.Body);
        }

        public void Post(DeleteNote request)
        {
            var crm = GetAuthenticatedCrmWrapper(request);
            crm.DeleteNote(request.NoteId);
        }

        public NoteWrapper Post(AddNote request)
        {
            var crm = GetAuthenticatedCrmWrapper(request);
            var incident = crm.GetIncident(request.CaseNum);
            return crm.AddNote(incident.Id, request.Title, request.Body);
        }

        public object Get(AttachmentRequest request)
        {
            var incident = CrmWrapper.Instance.GetIncident(request.CaseNum);
            var file = incident.NetworkAttachments.First(x => x.Filename.Equals(request.Filename));
            base.Response.AddHeader("Content-Disposition", "attachment");
            return new HttpResult(File.OpenRead(file.Path), "application/octet-steam");
        }

        public object Get(AttachmentFileRequest request)
        {
            Guid attachmentId;
            if (Guid.TryParse(request.AttachmentId, out attachmentId))
            {
                AttachmentFileWrapper result = CrmWrapper.Instance.GetAttachmentFile(attachmentId);
                if (result != null)
                {
                    base.Response.AddHeader("Content-Disposition", "attachment");
                    return new HttpResult(result.FileData, result.MimeType);
                }
            }
            return null;
        }

        public MatterMostResponse Post(MatterMostMessage request)
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

}
