using System;

namespace PDMS.Domain.Common
{
    public class BaseEntity
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastModified { get; set; }

        public string LastModifiedBy { get; set; }
    }
}