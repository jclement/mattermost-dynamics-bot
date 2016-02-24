using System;
using Microsoft.Xrm.Sdk;

namespace MattermostCrmService.Wrappers
{

    public class NoteWrapper
    {
        public NoteWrapper(Entity annotation, CrmWrapper wrapper)
        {
            Annotation note = (Annotation) annotation;
            Body = note.NoteText;;
            Filename = note.FileName;
            Filesize = note.FileSize;
            Mimetype = note.MimeType;
            Title = note.Subject ?? "Mysterious Untitled Note";
            Owner = wrapper.LookupUser(note.OwningUser);
            Created = note.CreatedOn.Value;
            Modified = note.ModifiedOn.Value;
            Id = note.Id.ToString("d");
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
