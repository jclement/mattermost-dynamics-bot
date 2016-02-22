using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MattermostCrmService.Messages
{
    [Route("/incident/search/{query}")]
    public class BasicIncidentQuery : IncidentQueryBase
    {
    }
}
