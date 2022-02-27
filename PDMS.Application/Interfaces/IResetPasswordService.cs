using PDMS.Domain.Models;

namespace PDMS.Application.Interfaces
{
    public interface IResetPasswordService
    {
        Task<ResetPasswordServiceResult> SendPasswordResetLink(string emailAddress, string passwordResetLink, string randomToken);
        Task<bool> IsUsernameInUse(string emailAddress);
        Task<string> GenerateRandomToken();
        Task<ResetPasswordServiceResult> SetNewPassword(string emailAddress, string returnToken, string newPassword);
    } 
}