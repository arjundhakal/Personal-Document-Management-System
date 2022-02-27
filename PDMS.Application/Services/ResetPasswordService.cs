using PDMS.Application.Interfaces;
using PDMS.Domain.Models;
using PDMS.Domain.Entity;
using Serilog;

namespace PDMS.Application.Services
{
    public class ResetPasswordService : IResetPasswordService
    {
        private const string EmailCouldnotBeSent = "Password reset link could not be sent in Email!";
        private const string PasswordChangeError = "Password couldnot be changed for user.";
        private const string CouldNotUpdateInRepository = "Couldnot update user in Repository.";
        private const string CharactersForRandomToken = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        private const string TokenExpiredMessage = "Invalid or expired token.";

        private readonly IDatabaseRepository<User> _userRepository;
        private readonly IAuthenticationClient _authenticationClient;
        private readonly IEmailServiceClient _emailServiceClient;

        public ResetPasswordService(IAuthenticationClient authenticationClient, IEmailServiceClient emailServiceClient, IDatabaseRepository<User> userRepository)
        {
            _userRepository = userRepository;
            _authenticationClient = authenticationClient;
            _emailServiceClient = emailServiceClient;
        }

        public async Task<ResetPasswordServiceResult> SendPasswordResetLink(string emailAddress, string passwordResetLink, string randomToken)
        {
            var UserDetail = await _userRepository.SingleOrDefaultAsync(x => x.Email.ToUpper() == emailAddress.ToUpper());
            var emailServiceResponse = await _emailServiceClient.SendEmail(emailAddress, UserDetail.FirstName + " " + UserDetail.LastName, passwordResetLink);

            if (!emailServiceResponse.IsSuccessful())
            {
                Log.Information("Password reset email could not be sent for user: {0}", emailAddress);
                return new ResetPasswordServiceResult()
                {
                    ErrorMessage = EmailCouldnotBeSent
                };
            }
            Log.Information("Password reset email sent for user: {0}", emailAddress);

            try
            {
                await UpdateUsersToken(emailAddress, randomToken);
                Log.Information("Updated user details for user {0} in Repository.", emailAddress);
            }
            catch (Exception ex)
            {
                Log.Information("couldnot update user details for user {0} in Repository.", emailAddress);
                return new ResetPasswordServiceResult()
                {
                    ErrorMessage = CouldNotUpdateInRepository
                };
            }

            return new ResetPasswordServiceResult()
            {
                ErrorMessage = null
            };
        }

        public async Task<bool> IsUsernameInUse(string emailAddress)
        {
            var RegisteredUser = await _userRepository.SingleOrDefaultAsync(x => x.Email.ToUpper() == emailAddress.ToUpper());

            if (RegisteredUser == null)
                return false;
            else
                return true;
        }

        public async Task<string> GenerateRandomToken()
        {
            var chars = CharactersForRandomToken;
            var stringChars = new char[8];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            return new String(stringChars);
        }

        protected async Task UpdateUsersToken(string emailAddress, string randomToken)
        {
            var UserDetails = await _userRepository.SingleOrDefaultAsync(x => x.Email.ToUpper() == emailAddress.ToUpper());
            UserDetails.PasswordResetTokenExpiry = DateTime.Now.AddHours(24);
            UserDetails.PasswordResetToken = randomToken;
            await _userRepository.Update(UserDetails);
        }

        public async Task<ResetPasswordServiceResult> SetNewPassword(string emailAddress, string returnToken, string newPassword)
        {
            var userDetails = await _userRepository.SingleOrDefaultAsync(x => x.Email.ToUpper() == emailAddress.ToUpper());

            if (String.IsNullOrEmpty(userDetails.AuthenticationIdentifier))
            {
                Log.Information("User: {0} doesnot exists.", emailAddress);
                return new ResetPasswordServiceResult()
                {
                    ErrorMessage = PasswordChangeError
                };
            }

            if (!(userDetails.PasswordResetToken == returnToken && userDetails.PasswordResetTokenExpiry >= DateTime.Now))
            {
                Log.Information("Invalid/expired token for User: {0}.", emailAddress);
                return new ResetPasswordServiceResult()
                {
                    ErrorMessage = TokenExpiredMessage
                };
            }

            var passwordChangeResponse = await _authenticationClient.ChangePassword(userDetails.AuthenticationIdentifier, newPassword);
            if (!passwordChangeResponse.IsSuccessful())
            {
                Log.Information("Password couldnot be changed for user: {0}", emailAddress);
                return new ResetPasswordServiceResult()
                {
                    ErrorMessage = PasswordChangeError
                };
            }
            Log.Information("Password is changed for user: {0} in auth0", emailAddress);

            try
            {
                await DisableUsersToken(emailAddress);
                Log.Information("Disabled password reset token for user {0} in Repository.", emailAddress);
            }
            catch (Exception ex)
            {
                Log.Information("couldnot disabled password reset token for user {0} in Repository.", emailAddress);
                return new ResetPasswordServiceResult()
                {
                    ErrorMessage = CouldNotUpdateInRepository
                };
            }

            return new ResetPasswordServiceResult()
            {
                ErrorMessage = null
            };
        }

        protected async Task DisableUsersToken(string emailAddress)
        {
            var userDetails = await _userRepository.SingleOrDefaultAsync(x => x.Email.ToUpper() == emailAddress.ToUpper());
            userDetails.LastModified = DateTime.Now;
            userDetails.LastModifiedBy = emailAddress;
            userDetails.PasswordResetTokenExpiry = DateTime.Now;
            await _userRepository.Update(userDetails);
        }
    }
}