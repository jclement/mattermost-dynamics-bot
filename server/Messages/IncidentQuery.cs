using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MattermostCrmService.Messages
{
    [Route("/incident/search")]
    public class IncidentQuery 
    {
        public string Query { get; set; }
        public Guid? OwnerId { get; set; }
        public Int32? StateCode { get; set; }
    }
}
