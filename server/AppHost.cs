using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack;

namespace MattermostCrmService
{
    public class AppHost : AppSelfHostBase
    {
        public AppHost() : base("HttpListener Self-Host", typeof(CRMService).Assembly)
        {
        }

        public override void Configure(Funq.Container container)
        {
            SetConfig(new HostConfig
            {
                AllowRouteContentTypeExtensions = false,
#if DEBUG
                DebugMode = true,
                WebHostPhysicalPath = "~/../..".MapServerPath(),
#endif
            });
        }
    }

}
