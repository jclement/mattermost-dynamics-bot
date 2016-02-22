using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MattermostCrmService.Messages
{
    [Route("/notes/{NoteId}")]
    public class UpdateNote : AuthenticatedRequestBase
    {
        public Guid NoteId { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
    }
}
