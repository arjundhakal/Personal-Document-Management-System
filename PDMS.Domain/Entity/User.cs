using PDMS.Domain.Common;

namespace PDMS.Domain.Entity
{
    public class User : BaseEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string AuthenticationIdentifier { get; set; }
        public string AuthenticationSource { get; set; }
        public bool IsAccountActive{ get; set; }
        public int FailedLoginAttempts { get; set; }
        public string AccountInactiveReason { get; set; }
        public string PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiry { get; set; }

    }
}