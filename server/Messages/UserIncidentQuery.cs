using ServiceStack;
using System;

namespace MattermostCrmService.Messages
{
    [Route("/incident/userSearch/{userId}")]
    [Route("/incident/userSearch/{userId}/{query}")]
    public class UserIncidentQuery : IncidentQueryBase
    {
        public Guid UserId { get; set; }
    }
}
