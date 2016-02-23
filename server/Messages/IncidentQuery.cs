using System;
using ServiceStack;

namespace MattermostCrmService.Messages
{
    [Route("/search/incident")]
    public class IncidentQuery 
    {
        public string Query { get; set; }
        public Guid? OwnerId { get; set; }
        public Int32? StateCode { get; set; }
    }
}
