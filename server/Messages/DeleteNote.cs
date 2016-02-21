using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MattermostCrmService.Messages
{
    [Route("/notes/{NoteId}/delete")]
    public class DeleteNote : AuthenticatedRequestBase
    {
        public Guid NoteId { get; set; }
    }
}
