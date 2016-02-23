using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack;

namespace MattermostCrmService.Messages
{
    [Route("/users/{Query}")]
    [Route("/users")]
    public class Users
    {
        public string Query { get; set; }
    }
}
