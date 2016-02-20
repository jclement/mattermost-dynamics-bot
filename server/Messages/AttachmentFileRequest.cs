using ServiceStack;

namespace MattermostCrmService.Messages
{
    [Route("/attachment/getfile/{AttachmentId}/{FileName}")]
    public class AttachmentFileRequest
    {
        public string AttachmentId { get; set; }
        public string FileName { get; set; }
    }
}