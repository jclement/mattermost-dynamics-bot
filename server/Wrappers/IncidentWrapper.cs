using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using Microsoft.Xrm.Sdk;

namespace MattermostCrmService.Wrappers
{

    public class IncidentWrapper : SlimIncidentWrapper
    {
        public IncidentWrapper(Entity incident, CrmWrapper wrapper): base(incident, wrapper)
        {

            Notes = wrapper.GetNotes(incident.Id).ToArray();

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

        public NoteWrapper[] Notes { get; set; }
        public NetworkAttachmentWrapper[] NetworkAttachments { get; set; }
    }

}
