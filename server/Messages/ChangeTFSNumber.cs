using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack;

namespace MattermostCrmService.Messages
{
    [Route("/incident/{CaseNum}/changetfsnumber")]
    public class ChangeTFSNumber : AuthenticatedRequestBase
    {
        public string TFSNumber { get; set; }
        public string CaseNum { get; set; }
    }
}
