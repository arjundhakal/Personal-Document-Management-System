using PDMS.Domain.Common;

namespace PDMS.Domain.Entity
{
    public class DocumentDetail : BaseEntity
    {
        public string FileName { get; set; }
        public string Identifier { get; set; }
    }
}