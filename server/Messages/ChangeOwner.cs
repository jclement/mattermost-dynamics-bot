using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack;

namespace MattermostCrmService.Messages
{
    [Route("/incident/{CaseNum}/changeowner")]
    public class ChangeOwner : AuthenticatedRequestBase
    {
        public string OwnerId { get; set; }
        public string CaseNum { get; set; }
    }
}
