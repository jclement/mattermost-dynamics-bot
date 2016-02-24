using ServiceStack;

namespace MattermostCrmService.Messages
{
    [Route("/mattermost/myTickets")]
    public class MatterMostMyIncidents
    {
        public string text { get; set; }
        public string user_name { get; set; }
    }
}