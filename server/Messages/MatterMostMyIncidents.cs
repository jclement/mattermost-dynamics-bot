using ServiceStack;

namespace MattermostCrmService.Messages
{
    [Route("/mattermost/myTickets")]
    public class MatterMostMyIncidents : MatterMostRequestBase
    {
        public string text { get; set; }
        public string user_name { get; set; }
    }
}