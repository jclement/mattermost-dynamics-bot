using ServiceStack;

namespace MattermostCrmService.Messages
{
    [Route("/mattermost/myTickets")]
    public class MattermostMyIncidents : MattermostRequestBase
    {
        public string text { get; set; }
        public string user_name { get; set; }
    }
}