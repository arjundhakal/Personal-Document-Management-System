using PDMS.Application.Repositories;
using PDMS.Domain.Entity;
using PDMS.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Xunit;
using PDMS.Infrastructure;
using PDMS.Application.Interfaces;
using System.Threading.Tasks;
using PDMS.Application.Services;

namespace PDMS.Test.Application.Services
{
    public class AuthenticationServiceTest
    {
        private DbContextOptions<ApplicationDbContext> _options;
        private ApplicationDbContext _dbContex;
        private DatabaseRepository<User> _userRepo;
        private DatabaseRepository<Session> _sessionRepo;
        private string _dbName;
        private AuthenticationService _authenticationService;
        private AuthenticationService _authenticationService2;

        public AuthenticationServiceTest()
        {
            _dbName = Guid.NewGuid().ToString();
            _options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(_dbName).Options;
            _dbContex = new ApplicationDbContext(_options);
            _userRepo = new DatabaseRepository<User>(_dbContex);
            _sessionRepo = new DatabaseRepository<Session>(_dbContex);
            _authenticationService = new AuthenticationService(_userRepo, _sessionRepo, new FakeAuthenticationClient());
            _authenticationService2 = new AuthenticationService(_userRepo, _sessionRepo, new NegFakeAuthenticationClient());
        }

        [Fact]
        public async void SuccessfullyCreateNewUser()
        {
            var firstName = Guid.NewGuid().ToString();
            var lastName = Guid.NewGuid().ToString();
            var emailAddress = Guid.NewGuid().ToString();
            var password = Guid.NewGuid().ToString();

            var AuthenticationCreatedUserResult = await _authenticationService.CreateUser(firstName, lastName, emailAddress, password);

            AuthenticationCreatedUserResult.Should().NotBeNull();
            AuthenticationCreatedUserResult.ErrorMessage.Should().BeNullOrEmpty();
            AuthenticationCreatedUserResult.IsSuccessful().Should().BeTrue();
        }

        [Fact]
        public async void ShouldShowErrorOnIssueWhileCreatingNewUser()
        {
            var firstName = Guid.NewGuid().ToString();
            var lastName = Guid.NewGuid().ToString();
            var emailAddress = Guid.NewGuid().ToString();
            var password = Guid.NewGuid().ToString();

            var AuthenticationCreatedUserResult = await _authenticationService2.CreateUser(firstName, lastName, emailAddress, password);

            AuthenticationCreatedUserResult.Should().NotBeNull();
            AuthenticationCreatedUserResult.ErrorMessage.Should().NotBeNullOrEmpty();
            AuthenticationCreatedUserResult.IsSuccessful().Should().BeFalse();
        }

        [Fact]
        public async void SuccessfullyLoginUser()
        {
            var firstName = Guid.NewGuid().ToString();
            var lastName = Guid.NewGuid().ToString();
            var emailAddress = Guid.NewGuid().ToString();
            var password = Guid.NewGuid().ToString();
            var ipAddress = Guid.NewGuid().ToString();

            var AuthenticationCreatedUserResult = await _authenticationService.CreateUser(firstName, lastName, emailAddress, password);

            var AuthenticationLoginUserResult = await _authenticationService.Login(emailAddress, password, ipAddress);

            AuthenticationCreatedUserResult.Should().NotBeNull();
            AuthenticationCreatedUserResult.ErrorMessage.Should().BeNullOrEmpty();
            AuthenticationCreatedUserResult.IsSuccessful().Should().BeTrue();

            AuthenticationLoginUserResult.Should().NotBeNull();
            AuthenticationLoginUserResult.ErrorMessage.Should().BeNullOrEmpty();
            AuthenticationLoginUserResult.IsSuccessful().Should().BeTrue();
        }

        [Fact]
        public async void UnsuccessfulLoginWithWrongCrediantials()
        {
            var firstName = Guid.NewGuid().ToString();
            var lastName = Guid.NewGuid().ToString();
            var emailAddress = Guid.NewGuid().ToString();
            var password = Guid.NewGuid().ToString();
            var ipAddress = Guid.NewGuid().ToString();
            var wrongPassword = Guid.NewGuid().ToString();
            var wrongEmailAddress = Guid.NewGuid().ToString();

            var AuthenticationCreatedUserResult = await _authenticationService.CreateUser(firstName, lastName, emailAddress, password);

            var AuthenticationLoginUserResult1 = await _authenticationService2.Login(emailAddress, wrongPassword, ipAddress);

            var AuthenticationLoginUserResult2 = await _authenticationService2.Login(wrongEmailAddress, password, ipAddress);

            var userDetails = await _userRepo.SingleOrDefaultAsync(x => x.Email.ToUpper() == emailAddress.ToUpper());
            var failedLoginAttempts = userDetails.FailedLoginAttempts;

            AuthenticationCreatedUserResult.Should().NotBeNull();
            AuthenticationCreatedUserResult.ErrorMessage.Should().BeNullOrEmpty();
            AuthenticationCreatedUserResult.IsSuccessful().Should().BeTrue();

            AuthenticationLoginUserResult1.Should().NotBeNull();
            AuthenticationLoginUserResult1.ErrorMessage.Should().NotBeNullOrEmpty();
            AuthenticationLoginUserResult1.IsSuccessful().Should().BeFalse();

            AuthenticationLoginUserResult2.Should().NotBeNull();
            AuthenticationLoginUserResult2.ErrorMessage.Should().NotBeNullOrEmpty();
            AuthenticationLoginUserResult2.IsSuccessful().Should().BeFalse();
            
            failedLoginAttempts.Should().BeGreaterThan(0);
        }

        [Fact]
        public async void SuccessfullyLogOutUser()
        {
            var firstName = Guid.NewGuid().ToString();
            var lastName = Guid.NewGuid().ToString();
            var emailAddress = Guid.NewGuid().ToString();
            var password = Guid.NewGuid().ToString();
            var ipAddress = Guid.NewGuid().ToString();

            var AuthenticationCreatedUserResult = await _authenticationService.CreateUser(firstName, lastName, emailAddress, password);

            var AuthenticationLoginUserResult = await _authenticationService.Login(emailAddress, password, ipAddress);

            var userDetails = await _userRepo.SingleOrDefaultAsync(x => x.Email.ToUpper() == emailAddress.ToUpper());
            var sessionDetails = await _sessionRepo.SingleOrDefaultAsync(x => x.UserId == userDetails.Id);

            var AuthenticationResult = await _authenticationService.Logout(sessionDetails.SessionId.ToString());

            AuthenticationCreatedUserResult.Should().NotBeNull();
            AuthenticationCreatedUserResult.ErrorMessage.Should().BeNullOrEmpty();
            AuthenticationCreatedUserResult.IsSuccessful().Should().BeTrue();

            AuthenticationLoginUserResult.Should().NotBeNull();
            AuthenticationLoginUserResult.ErrorMessage.Should().BeNullOrEmpty();
            AuthenticationLoginUserResult.IsSuccessful().Should().BeTrue();

            AuthenticationResult.Should().NotBeNull();
            AuthenticationResult.ErrorMessage.Should().BeNullOrEmpty();
            AuthenticationResult.IsSuccessful().Should().BeTrue();
        }
    }

    #region FakeInterfaces
    public class NegFakeAuthenticationClient : IAuthenticationClient
    {
        public async Task<AuthenticationCreatedUserResult> CreateUser(string username, string password)
        {
            return new AuthenticationCreatedUserResult() { ErrorMessage = "Error in creating user."};
        }
        public async Task<AuthenticationLoginUserResult> Login(string username, string password)
        {
            return new AuthenticationLoginUserResult() { ErrorMessage = "Error in login." };
        }

        public async Task<AuthenticationCreatedUserResult> ChangePassword(string identifier, string newPassword)
        {
            return new AuthenticationCreatedUserResult() { ErrorMessage = "Error while changing password." };
        }
    }
    #endregion
}