using PDMS.Domain.Common;

namespace PDMS.Domain.Entity
{
    public class Setting : BaseEntity
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}