using System;
using Microsoft.Xrm.Sdk;

namespace MattermostCrmService.Wrappers
{

    public class NoteWrapper
    {
        public NoteWrapper(Entity annotation, CrmWrapper wrapper)
        {
            Body = annotation.GetAttributeValue<string>("notetext");
            Filename = annotation.GetAttributeValue<string>("filename");
            Filesize = annotation.GetAttributeValue<Int32>("filesize");
            Mimetype = annotation.GetAttributeValue<string>("mimetype");
            Title = annotation.GetAttributeValue<string>("subject") ?? "Mysterious Untitled Note";
            Owner = wrapper.LookupUser(annotation.GetAttributeValue<EntityReference>("owninguser"));
            Created = annotation.GetAttributeValue<DateTime>("createdon");
            Modified = annotation.GetAttributeValue<DateTime>("modifiedon");
            Id = annotation.Id.ToString("d");
        }
        public string Title { get; set; }
        public string Body { get; set; }
        public string Owner { get; set; }
        public string Filename { get; set; }
        public Int32? Filesize { get; set; }
        public string Mimetype { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Modified { get; set; }
        public string Id { get; set; }
    }
}
