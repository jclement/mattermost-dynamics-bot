using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JsonConfig;
using MattermostCrmService.Wrappers;

namespace MattermostCrmService
{
    public class IncidentMarkdowner
    {
        public static string ConvertToMarkdown(SlimIncidentWrapper incident)
        {
            StringBuilder sb = new StringBuilder();
            if (incident != null)
            {
                sb.AppendLine($"__Case__ : [{incident.TicketNumber}]({Config.MergedConfig.WebRoot}/static/#/incident/{incident.TicketNumber})");
                sb.AppendLine($"__Title__ : {incident.Title}");
                sb.AppendLine($"__Company__ : {incident.Company}");
                sb.AppendLine($"__Owner__ : {incident.Owner}");
                sb.AppendLine($"__Description__ : {incident.Description}");
            }
            return sb.ToString();
        }
    }
}
