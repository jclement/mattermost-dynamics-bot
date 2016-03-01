using ServiceStack;

namespace MattermostCrmService.Messages
{
    [Route("/receivemessage")]
    public class MattermostMessage : MattermostRequestBase
    {
        public string text { get; set; }
        public string user_name { get; set; }
    }
}