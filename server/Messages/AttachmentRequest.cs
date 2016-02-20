using ServiceStack;

namespace MattermostCrmService.Messages
{
    [Route("/incident/{CaseNum}/attachments/{Filename}")]
    public class AttachmentRequest
    {
        public string CaseNum { get; set; }
        public string Filename { get; set; }
    }
}