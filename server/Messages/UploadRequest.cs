using ServiceStack;

namespace MattermostCrmService.Messages
{
    [Route("/incident/{CaseNum}/uploadFiles")]
    public class UploadRequest : AuthenticatedRequestBase
    {
        public string CaseNum { get; set; }
    }
}