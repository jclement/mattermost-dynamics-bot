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
            NetworkAttachments = NetworkAttachment.ListAttachments(NetworkAttachmentsFolder);
        }

        public NoteWrapper[] Notes { get; set; }
        public NetworkAttachmentWrapper[] NetworkAttachments { get; set; }
    }

}
