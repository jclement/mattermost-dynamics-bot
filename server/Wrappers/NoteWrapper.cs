using System;
using Microsoft.Xrm.Sdk;
using System.Xml;
using System.Linq;
using System.Xml.Linq;

namespace MattermostCrmService.Wrappers
{
    public enum NoteType
    {
        Note = 1,
        AutomaticPost = 2,
        ManualPost = 3
    }

    public class NoteWrapper
    {
        private void ProcessAutomaticPost(string post)
        {
            /// <?xml version="1.0" encoding="utf-16"?><pi id="CaseAssign.Post" icon="0"><ps><p type="1" otc="112" id="dfaa408a-c88e-e511-80e5-3863bb2eb148">Upgrade from 8.0 to 8.1</p><p type="1" otc="8" id="dedbc981-a413-e511-80ff-c4346bad87a4">Chris Rowat</p><p type="1" otc="8" id="f2dbc981-a413-e511-80ff-c4346bad87a4">Mark Dryden</p></ps></pi>
            /// 

            var doc = XDocument.Parse(post);
            var id = doc.Root.Attribute("id").Value.ToUpperInvariant();

            if (id == "CASEASSIGN.POST")
            {
                var toUser = doc.Root.Elements("ps").Elements("p")
                    .Where(node => (string) node.Attribute("type") == "1" && (string) node.Attribute("otc") == "8")
                    .First().Value;
                var fromUser = doc.Root.Elements("ps").Elements("p")
                    .Where(node => (string) node.Attribute("type") == "1" && (string) node.Attribute("otc") == "8")
                    .Skip(1).First().Value;
                Title = $"Ownership changed to {toUser}";
                Body = null;
                Owner = fromUser;
            }

            if (id == "ACCOUNT.CASECLOSE.POST")
            {
                var user = doc.Root.Elements("ps").Elements("p")
                    .Where(node => (string) node.Attribute("type") == "1" && (string) node.Attribute("otc") == "8")
                    .First().Value;
                Title = "Case closed";
                Body = null;
                Owner = user;
            }

            if (id == "ACCOUNT.CASECREATE.POST")
            {
                var user = doc.Root.Elements("ps").Elements("p")
                    .Where(node => (string) node.Attribute("type") == "1" && (string) node.Attribute("otc") == "8")
                    .First().Value;
                Title = "Case created";
                Body = null;
                Owner = user;
            }

            if (id == "CASE.CASEREACTIVATE.POST")
            {
                var user = doc.Root.Elements("ps").Elements("p")
                    .Where(node => (string) node.Attribute("type") == "1" && (string) node.Attribute("otc") == "8")
                    .First().Value;
                Title = "Case ressurected";
                Body = null;
                Owner = user;
            }

        }
        private string StripTags(string HTML)
        {
            // Removes tags from passed HTML            
            System.Text.RegularExpressions.Regex objRegEx = new System.Text.RegularExpressions.Regex("<[^>]*>");
            return objRegEx.Replace(HTML, "");
        }
        public NoteWrapper(Post post, CrmWrapper wrapper)
        {
            Title = post.SourceEnum == Post_Source.AutoPost ? "Automatic Post" : "User Post";
            Body = post.Text;
            Owner = wrapper.LookupUser(post.CreatedBy);
            Created = post.CreatedOn.Value;
            Modified = post.ModifiedOn.Value;
            Id = post.Id.ToString("d");
            NoteType = post.SourceEnum == Post_Source.AutoPost ? NoteType.AutomaticPost : NoteType.ManualPost;
            Editable = false;
            if (post.SourceEnum == Post_Source.AutoPost)
            {
                ProcessAutomaticPost(post.Text);
            }
        }
        public NoteWrapper(Annotation note, CrmWrapper wrapper)
        {
            Body = note.NoteText;;
            Filename = note.FileName;
            Filesize = note.FileSize;
            Mimetype = note.MimeType;
            Title = note.Subject ?? "Mysterious Untitled Note";
            Owner = wrapper.LookupUser(note.OwningUser);
            Created = note.CreatedOn.Value;
            Modified = note.ModifiedOn.Value;
            Id = note.Id.ToString("d");
            NoteType = NoteType.Note;
            Editable = true;
        }
        public string Title { get; set; }
        public string Body { get; set; }
        public string Owner { get; set; }
        public string Filename { get; set; }
        public Int32? Filesize { get; set; }
        public string Mimetype { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Modified { get; set; }
        public bool Editable { get; set; }
        public NoteType NoteType { get; set; }
        public string Id { get; set; }
    }
}
