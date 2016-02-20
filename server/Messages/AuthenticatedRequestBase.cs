using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MattermostCrmService.Messages
{
    public class AuthenticatedRequestBase
    {
        public string AuthenticationToken { get; set; }
    }
}
