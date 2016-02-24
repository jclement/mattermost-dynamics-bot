using ServiceStack;

namespace MattermostCrmService.Messages
{
    [Route("/incident/{CaseNum}")]
    public class IncidentMessage
    {
        public string CaseNum { get; set; }
    }
}