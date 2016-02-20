using System.IO;

namespace MattermostCrmService.Wrappers
{
    public class AttachmentFileWrapper
    {
        public Stream FileData { get; set; }
        public string MimeType { get; set; }
    }
}
