using PDMS.Domain.Common;

namespace PDMS.Domain.Entity
{
    public class Session : BaseEntity
    {
        public int UserId { get; set; }
        public Guid SessionId { get; set; }
        public string IPAddress { get; set; }
        public DateTime ExpireAt { get; set; }
        public int? CurrentActiveOrganisationId { get; set; }
    }
}