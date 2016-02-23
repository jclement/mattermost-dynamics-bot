using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace MattermostCrmService
{
    public class NetworkAttachment
    {
        public static NetworkAttachmentWrapper[] ListAttachments(string folder)
        {
            var attachments = new List<NetworkAttachmentWrapper>();
            if (!string.IsNullOrEmpty(folder) && Directory.Exists(folder))
            {
                foreach (var path in Directory.GetFiles(folder))
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
                        SizeKB = (info.Length + 1023) / 1024,
                        Owner = sec.GetOwner(typeof(SecurityIdentifier)).Translate(typeof(NTAccount)).Value
                    });
                }
            }
            return attachments.ToArray();
        }
    }
}
