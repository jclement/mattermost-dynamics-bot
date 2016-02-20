using ServiceStack;

namespace MattermostCrmService.Messages
{
    [Route("/incident/{CaseNum}")]
    public class Incident
    {
        public string CaseNum { get; set; }
    }
}