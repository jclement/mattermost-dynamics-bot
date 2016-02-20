using ServiceStack;

namespace MattermostCrmService.Messages
{
    [Route("/incident/{CaseNum}/notes/add")]
    public class AddNote : AuthenticatedRequestBase
    {
        public string CaseNum { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
    }
}