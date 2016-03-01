using ServiceStack;

namespace MattermostCrmService.Messages
{
    [Route("/incident/{CaseNum}")]
    public class IncidentMessage : AuthenticatedRequestBase
    {
        public string CaseNum { get; set; }
    }
}