using PDMS.Domain.Common;
using PDMS.Domain.Models;

namespace PDMS.Domain.Entity
{
    public class DocumentStorageResult : BaseResult
    {
        public string Source { get; set; }
        public string Identifier { get; set; }
        public string Payload { get; set; }
    }
}