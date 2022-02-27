using PDMS.Application.Repositories;
using PDMS.Application.Services;
using PDMS.Domain.Entity;
using PDMS.Domain.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using Xunit;
using PDMS.Infrastructure;
using PDMS.Application.Interfaces;
using System.Threading.Tasks;

namespace PDMS.Test.Application.Services
{
    public class ResetPasswordServiceTest
    {
        private DbContextOptions<ApplicationDbContext> _options;
        private ApplicationDbContext _dbContex;
        private DatabaseRepository<User> _userRepo;
        private string _dbName;
        private FakeResetPasswordService _resetPasswordService;
        private FakeResetPasswordService _resetPasswordService2;

        public ResetPasswordServiceTest()
        {
            _dbName = Guid.NewGuid().ToString();
            _options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(_dbName).Options;
            _dbContex = new ApplicationDbContext(_options);
            _userRepo = new DatabaseRepository<User>(_dbContex);
            _resetPasswordService = new FakeResetPasswordService(new FakeAuthenticationClient(), new FakeGmailServiceClient(), _userRepo);
            _resetPasswordService2 = new FakeResetPasswordService(new FakeAuthenticationClient(), new NegFirstFakeGmailServiceClient(), _userRepo);
        }

        [Fact]
        public async void SuccessfullySendPasswordResetLink()
        {
            var emailAddress = Guid.NewGuid().ToString();
            var passwordResetLink = Guid.NewGuid().ToString();
            var randomToken = Guid.NewGuid().ToString();
           
            await _userRepo.Create(new User()
            {
                FirstName = Guid.NewGuid().ToString(),
                LastName = Guid.NewGuid().ToString(),
                Email = emailAddress
            });

            var ResetPasswordServiceResult = await _resetPasswordService.SendPasswordResetLink(emailAddress, passwordResetLink, randomToken);

            ResetPasswordServiceResult.Should().NotBeNull();
            ResetPasswordServiceResult.ErrorMessage.Should().BeNullOrEmpty();
            ResetPasswordServiceResult.IsSuccessful().Should().BeTrue();
        }

        [Fact]
        public async void UnSuccessfulToSendPasswordResetLink()
        {
            var emailAddress = Guid.NewGuid().ToString();
            var passwordResetLink = Guid.NewGuid().ToString();
            var randomToken = Guid.NewGuid().ToString();

            await _userRepo.Create(new User()
            {
                FirstName = Guid.NewGuid().ToString(),
                LastName = Guid.NewGuid().ToString(),
                Email = emailAddress
            });

            var ResetPasswordServiceResult = await _resetPasswordService2.SendPasswordResetLink(emailAddress, passwordResetLink, randomToken);

            ResetPasswordServiceResult.Should().NotBeNull();
            ResetPasswordServiceResult.ErrorMessage.Should().NotBeNullOrEmpty();
            ResetPasswordServiceResult.IsSuccessful().Should().BeFalse();
        }

        [Fact]
        public async void ShouldReturnTrueIfUsernameInUse()
        {
            var emailAddress = Guid.NewGuid().ToString();

            await _userRepo.Create(new User()
            {
                FirstName = Guid.NewGuid().ToString(),
                LastName = Guid.NewGuid().ToString(),
                Email = emailAddress
            });

            var ResetPasswordServiceResult = await _resetPasswordService.IsUsernameInUse(emailAddress);

            ResetPasswordServiceResult.Should().BeTrue();
        }

        [Fact]
        public async void ShouldReturnFalseIfUsernameNotInUse()
        {
            var emailAddress = Guid.NewGuid().ToString();

            var ResetPasswordServiceResult = await _resetPasswordService.IsUsernameInUse(emailAddress);

            ResetPasswordServiceResult.Should().BeFalse();
        }

        [Fact]
        public async void ShouldReturnRandomTokenOfLengthEight()
        {
            var ResetPasswordServiceResult = await _resetPasswordService.GenerateRandomToken();

            ResetPasswordServiceResult.Should().NotBeNull();
            ResetPasswordServiceResult.Length.Should().Be(8);
        }

        

        [Fact]
        public async void DisableUserTokenInRepo()
        {
            var emailAddress = Guid.NewGuid().ToString();

            await _userRepo.Create(new User()
            {
                FirstName = Guid.NewGuid().ToString(),
                LastName = Guid.NewGuid().ToString(),
                Email = emailAddress
            });

            await _resetPasswordService.DisableUsersToken(emailAddress);

            var userDetails = await _userRepo.SingleOrDefaultAsync(x => x.Email.ToUpper() == emailAddress.ToUpper());
            
            userDetails.Should().NotBeNull();
            userDetails.PasswordResetTokenExpiry.Should().NotBeOnOrAfter(DateTime.Now);
        }

        [Fact]
        public async void UpdateUserTokenInRepo()
        {
            var emailAddress = Guid.NewGuid().ToString();
            var randomToken = Guid.NewGuid().ToString();

            await _userRepo.Create(new User()
            {
                FirstName = Guid.NewGuid().ToString(),
                LastName = Guid.NewGuid().ToString(),
                Email = emailAddress
            });

            await _resetPasswordService.UpdateUsersToken(emailAddress, randomToken);

            var userDetails = await _userRepo.SingleOrDefaultAsync(x => x.Email.ToUpper() == emailAddress.ToUpper());

            userDetails.PasswordResetToken.Should().NotBeNull();
            userDetails.PasswordResetTokenExpiry.Should().NotBeNull();
            userDetails.PasswordResetTokenExpiry.Should().BeAfter(DateTime.Now);
            userDetails.PasswordResetTokenExpiry.Should().BeBefore(DateTime.Now.AddDays(1));
        }

        [Fact]
        public async void ShouldBeAbleSetNewPasswordWithValidToken()
        {
            var emailAddress = Guid.NewGuid().ToString();
            var returnToken = Guid.NewGuid().ToString();
            var newPassword = Guid.NewGuid().ToString();

            await _userRepo.Create(new User()
            {
                FirstName = Guid.NewGuid().ToString(),
                LastName = Guid.NewGuid().ToString(),
                Email = emailAddress,
                PasswordResetToken = returnToken,
                PasswordResetTokenExpiry = DateTime.Now.AddDays(1),
                AuthenticationIdentifier = Guid.NewGuid().ToString()
            });

            var ResetPasswordServiceResult = await _resetPasswordService.SetNewPassword(emailAddress, returnToken, newPassword);

            ResetPasswordServiceResult.Should().NotBeNull();
            ResetPasswordServiceResult.ErrorMessage.Should().BeNullOrEmpty();
            ResetPasswordServiceResult.IsSuccessful().Should().BeTrue();
        }

        [Fact]
        public async void ShouldNotBeAbleSetNewPasswordWithInvalidToken()
        {
            var emailAddress = Guid.NewGuid().ToString();
            var returnToken = Guid.NewGuid().ToString();
            var newPassword = Guid.NewGuid().ToString();
            var invalidReturnToken = Guid.NewGuid().ToString();

            await _userRepo.Create(new User()
            {
                FirstName = Guid.NewGuid().ToString(),
                LastName = Guid.NewGuid().ToString(),
                Email = emailAddress,
                PasswordResetToken = returnToken,
                PasswordResetTokenExpiry = DateTime.Now.AddDays(1),
                AuthenticationIdentifier = Guid.NewGuid().ToString()
            });

            var ResetPasswordServiceResultInvalidToken = await _resetPasswordService.SetNewPassword(emailAddress, invalidReturnToken, newPassword);

            var userDetails = await _userRepo.SingleOrDefaultAsync(x => x.Email.ToUpper() == emailAddress.ToUpper());
            userDetails.PasswordResetTokenExpiry = DateTime.Now;
            await _userRepo.Update(userDetails);

            var ResetPasswordServiceResultExpiredToken = await _resetPasswordService.SetNewPassword(emailAddress, returnToken, newPassword);

            ResetPasswordServiceResultInvalidToken.Should().NotBeNull();
            ResetPasswordServiceResultInvalidToken.ErrorMessage.Should().NotBeNullOrEmpty();
            ResetPasswordServiceResultInvalidToken.IsSuccessful().Should().BeFalse();

            ResetPasswordServiceResultExpiredToken.Should().NotBeNull();
            ResetPasswordServiceResultExpiredToken.ErrorMessage.Should().NotBeNullOrEmpty();
            ResetPasswordServiceResultExpiredToken.IsSuccessful().Should().BeFalse();
        }
    }

    public class FakeResetPasswordService : ResetPasswordService
    {
        public FakeResetPasswordService(IAuthenticationClient authenticationClient, 
            IEmailServiceClient emailServiceClient, IDatabaseRepository<User> userRepository) : base(authenticationClient, emailServiceClient, userRepository)
        {

        }
        public async Task DisableUsersToken(string emailAddress) => await base.DisableUsersToken(emailAddress);
        public async Task UpdateUsersToken(string emailAddress, string randomToken) => await base.UpdateUsersToken(emailAddress, randomToken);
    }


    #region FakeInterfaces

    public class FakeGmailServiceClient : IEmailServiceClient
    {
        public async Task<EmailServiceResult> SendEmail(string toEmailAddress, string toUserFirstName, string PasswordResetLink)
        {
            return new EmailServiceResult() { ErrorMessage = null };
        }
    }

    public class NegFirstFakeGmailServiceClient : IEmailServiceClient
    {
        public Task<EmailServiceResult> SendEmail(string toEmailAddress, string toUserFirstName, string PasswordResetLink)
        {
            return Task.FromResult(new EmailServiceResult() { ErrorMessage = "Error : cannot send the email." });
        }
    }

    public class FakeAuthenticationClient : IAuthenticationClient
    {
        public async Task<AuthenticationCreatedUserResult> CreateUser(string username, string password)
        {
            return new AuthenticationCreatedUserResult() { Identifier = Guid.NewGuid().ToString() };
        }
        public async Task<AuthenticationLoginUserResult> Login(string username, string password)
        {
            return new AuthenticationLoginUserResult() { Identifier = Guid.NewGuid().ToString() };
        }

        public async Task<AuthenticationCreatedUserResult> ChangePassword(string identifier, string newPassword)
        {
            return new AuthenticationCreatedUserResult() { ErrorMessage = null };
        }
    }

    #endregion
}
