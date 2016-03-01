using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Text.RegularExpressions;
using JsonConfig;
using ServiceStack;
using MattermostCrmService.Wrappers;
using MattermostCrmService.Messages;
using System.Collections.Concurrent;

namespace MattermostCrmService
{
    public class CRMService : ServiceStack.Service
    {
        private static Regex CASRegex = new Regex("\\b(CAS-[0-9]{5}-[A-Z][0-9][A-Z][0-9][A-Z][0-9])\\b");

        public IncidentWrapper Any(IncidentMessage request)
        {
            return CrmConnectionManager.Instance.Get(request).GetIncident(request.CaseNum);
        }

        public LoginResponse Post(Login request)
        {
            try
            {
                var token = LoginHelper.Instance.GenerateToken(request.Username, request.Password);
                CrmConnectionManager.Instance.Get(token);
                return new LoginResponse()
                {
                    AuthenticationToken = token
                };
            }
            catch (Exception e)
            {
                throw new ApplicationException("Authentication Failure");
            }
        }

        public SlimIncidentWrapper[] Get(IncidentQuery request)
        {
            return CrmConnectionManager.Instance.Get(request).SearchIncidents(request.Query, request.OwnerId, request.StateCode).ToArray();
        }

        public object Get(Users request)
        {
            return CrmConnectionManager.Instance.Get(request).MatchUsersByName(request.Query);
        }

        public MattermostMyIncidentsResponse Post(MattermostMyIncidents request)
        {
            Dictionary<string, Guid> map = new Dictionary<string, Guid>();
            foreach (dynamic userMap in Config.MergedConfig.UserMap)
            {
                map.Add(userMap.name, Guid.Parse(userMap.crmid));
            }

            Guid ownerId;

            if (!map.TryGetValue(request.user_name, out ownerId))
            {
                return new MattermostMyIncidentsResponse()
                {
                    response_type = "ephemeral",
                    text = "I can't map " + request.user_name + " to a user in CRM"
                };
            }

            var incidents = CrmConnectionManager.Instance.MattermostInstance.SearchIncidents(request.text, ownerId, 1);

            StringBuilder output = new StringBuilder();

            output.AppendLine("Case | Title | Company ");
            output.AppendLine("---|---|---");
            foreach (var incident in incidents)
            {
                output.Append($"[{incident.TicketNumber}]({Config.MergedConfig.WebRoot}/static/#/incident/{incident.TicketNumber})");
                output.Append("|");
                output.Append(incident.Title);
                output.Append("|");
                output.Append(incident.Company);
                output.AppendLine("");
            }

            return new MattermostMyIncidentsResponse()
            {
                response_type = "ephemeral",
                text = output.ToString()
            };
        }

        public SlimIncidentWrapper Post(ChangeOwner request)
        {
            var crm = CrmConnectionManager.Instance.Get(request);
            var incident = crm.GetSlimIncident(request.CaseNum);
            return crm.UpdateOwner(Guid.Parse(request.OwnerId), incident.Id);
        }

        public NoteWrapper Post(UpdateNote request)
        {
            var crm = CrmConnectionManager.Instance.Get(request);
            return crm.UpdateNote(request.NoteId, request.Title, request.Body);
        }

        public void Post(DeleteNote request)
        {
            var crm = CrmConnectionManager.Instance.Get(request);
            crm.DeleteNote(request.NoteId);
        }

        public NoteWrapper Post(AddNote request)
        {
            var crm = CrmConnectionManager.Instance.Get(request);
            var incident = crm.GetIncident(request.CaseNum);
            return crm.AddNote(incident.Id, request.Title, request.Body);
        }

        public object Get(AttachmentRequest request)
        {
            var incident = CrmConnectionManager.Instance.Get(request).GetIncident(request.CaseNum);
            var file = incident.NetworkAttachments.First(x => x.Filename.Equals(request.Filename));
            base.Response.AddHeader("Content-Disposition", "attachment");
            return new HttpResult(File.OpenRead(file.Path), "application/octet-steam");
        }

        public object Get(AttachmentFileRequest request)
        {
            Guid attachmentId;
            if (Guid.TryParse(request.AttachmentId, out attachmentId))
            {
                AttachmentFileWrapper result = CrmConnectionManager.Instance.Get(request).GetAttachmentFile(attachmentId);
                if (result != null)
                {
                    base.Response.AddHeader("Content-Disposition", "attachment");
                    return new HttpResult(result.FileData, result.MimeType);
                }
            }
            return null;
        }

        public NetworkAttachmentWrapper[] Post(UploadRequest request)
        {
            var incident = CrmConnectionManager.Instance.Get(request).GetSlimIncident(request.CaseNum);
            if (string.IsNullOrEmpty(incident.NetworkAttachmentsFolder))
            {
                var crm = CrmConnectionManager.Instance.Get(request);

                var newDir = Path.Combine(Config.MergedConfig.NetworkAttachmentsBase, incident.TicketNumber);

                if (Directory.Exists(newDir))
                {
                    throw new DirectoryNotFoundException($"Path '{newDir}' already exists but is not specified in the 'Case Attachments' field");
                }

                Directory.CreateDirectory(newDir);
                
                incident = crm.UpdateNetworkAttachmentsFolder(newDir, incident.Id);
            }


            if (!Directory.Exists(incident.NetworkAttachmentsFolder))
            {
                throw new DirectoryNotFoundException($"Path '{incident.NetworkAttachmentsFolder}' does not exist or is not a directory");
            }

            var timestamp = DateTime.UtcNow;
            foreach (var uploadedFile in Request.Files.Where(uploadedFile => uploadedFile.ContentLength > 0))
            {
                string fileName = Path.GetFileName(uploadedFile.FileName); // removes any path info that naughty people may have stuck onto the file name.

                if (string.IsNullOrEmpty(fileName))
                {
                    fileName = "unknown";
                }

                if (File.Exists(Path.Combine(incident.NetworkAttachmentsFolder, fileName)))
                {
                    string baseNewName = fileName + "_previous";
                    string newName = baseNewName;
                    int counter = 2;
                    while (true)
                    {
                        if (!File.Exists(Path.Combine(incident.NetworkAttachmentsFolder, newName)))
                        {
                            break;
                        }

                        newName = baseNewName + $"({counter})";
                        counter = counter + 1;
                    }

                    File.Move(Path.Combine(incident.NetworkAttachmentsFolder, fileName), Path.Combine(incident.NetworkAttachmentsFolder, newName));
                }
                using (var stream = File.Create(Path.Combine(incident.NetworkAttachmentsFolder, fileName)))
                {
                    uploadedFile.WriteTo(stream);
                }
                File.SetCreationTimeUtc(Path.Combine(incident.NetworkAttachmentsFolder, fileName), timestamp);
                File.SetLastWriteTimeUtc(Path.Combine(incident.NetworkAttachmentsFolder, fileName), timestamp);
            }

            return NetworkAttachment.ListAttachments(incident.NetworkAttachmentsFolder);
        }

        public MattermostResponse Post(MattermostMessage request)
        {
            List<string> cases = new List<string>();
            HashSet<string> visitedCases = new HashSet<string>();
            foreach (Match match in CASRegex.Matches(request.text))
            {
                if (!visitedCases.Contains(match.Value))
                {
                    visitedCases.Add(match.Value);
                    IncidentWrapper result = CrmConnectionManager.Instance.MattermostInstance.GetIncident(match.Value);
                    var tmp = IncidentMarkdowner.ConvertToMarkdown(result);
                    if (!string.IsNullOrEmpty(tmp))
                    {
                        cases.Add(tmp);
                    }
                }
            }
            if (cases.Count > 0)
            {
                return new MattermostResponse { text = string.Join(Environment.NewLine, cases), username = "CRM-Bot", icon_url = Config.MergedConfig.WebRoot + "/static/reaper.png" };
            }
            return null;
        }
    }

}
