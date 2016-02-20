using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MattermostCrmService
{
    public class NetworkAttachmentWrapper
    {
        public string Filename { get; set; }
        public string Owner { get; set; }
        public string Path { get; set; }
        public DateTime Modified { get; set; }
        public DateTime Created { get; set; }
        public long SizeKB { get; set; }
    }
}
