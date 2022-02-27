using System;

namespace PDMS.UI.Models
{
    public class DocumentSearch

    {
        public int Id { get; set; }
        public string DocumentFileName { get; set; }
        public string Identifier { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastModified { get; set; }
        public string LastModifiedBy { get; set; }
    }
}