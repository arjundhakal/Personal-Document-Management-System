using PDMS.Application.Interfaces;
using PDMS.Domain.Entity;
using PDMS.Domain.Models;
using Serilog;

namespace PDMS.Application.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private const string InvalidUsernameOrPasswordError = "Invalid username or password.";
        private const string LogoutFailed = "Logout Failed.";
        private const string SystemDown = "System is currently down.";
        private const string RegistrationFailed = "New User Registration Failed.";
        private const string UsernameAlreadlyInUse = "An account with the entered email address already exists.";
        private const string UserBlockedMessage = "An account with the entered email address is blocked.";
        private const string AccountInactiveReason = "Username/password wrong more than 5 times.";
        private const int MaxLoginAttempts = 5;


        private readonly IDatabaseRepository<User> _userRepository;
        private readonly IDatabaseRepository<Session> _sessionRepository;

        private readonly IAuthenticationClient _authenticationClient;

        public AuthenticationService(IDatabaseRepository<User> userRepository,
                                     IDatabaseRepository<Session> sessionRepository,
                                     IAuthenticationClient authenticationClient)
        {
            _userRepository = userRepository;
            _sessionRepository = sessionRepository;
            _authenticationClient = authenticationClient;
        }

        public async Task<AuthenticationResult> CreateUser(string firstName, string lastName, string emailAddress, string password)
        {
            var userResults = await IsUserActive(emailAddress);
            if (userResults.isUserRegistered)
            {
                Log.Information("Could not create new account. An account with the {0} email address already exists.", emailAddress);
                return new AuthenticationResult()
                {
                    ErrorMessage = UsernameAlreadlyInUse
                };
            }

            var authenticationResponse = await _authenticationClient.CreateUser(emailAddress, password);

            if (!authenticationResponse.IsSuccessful())
            {
                Log.Information("New Registration failed for {0}.", emailAddress);
                return new AuthenticationResult()
                {
                    ErrorMessage = RegistrationFailed
                };
            }

            var user = GenerateUser(firstName, lastName, emailAddress, authenticationResponse.Identifier, authenticationResponse.Source);
            await _userRepository.Create(user);

            return new AuthenticationResult();
        }

        private User GenerateUser(string firstName, string lastName, string emailAddress, string authIdentifier, string authSource)
        {
            var user = new User()
            {
                FirstName = firstName,
                LastName = lastName,
                Email = emailAddress,
                LastModified = DateTime.Now,
                LastModifiedBy = "",
                AuthenticationIdentifier = authIdentifier,
                AuthenticationSource = authSource,
                IsAccountActive = true,
                AccountInactiveReason = "",
                FailedLoginAttempts = 0,
                PasswordResetToken = "",
                PasswordResetTokenExpiry = DateTime.Now.AddDays(-1)
            };

            return user;
        }


        public async Task<AuthenticationResult> Login(string emailAddress, string password, string ipAddress)
        {
            try
            {
                var userResults = await IsUserActive(emailAddress);
                if (!userResults.isUserRegistered)
                {
                    Log.Information("Log in Denied. An account with the {0} email address doesnot exists.", emailAddress);

                    return new AuthenticationResult()
                    {
                        ErrorMessage = InvalidUsernameOrPasswordError
                    };
                }

                if (!userResults.isUserActive)
                {
                    Log.Information("Log in Denied. An account with the entered email address {0} is blocked.", emailAddress);

                    return new AuthenticationResult()
                    {
                        ErrorMessage = UserBlockedMessage
                    };
                }

                var authenticationResponse = await _authenticationClient.Login(emailAddress, password);

                if (authenticationResponse.IsSuccessful())
                {
                    await ResetFailedLoginAttempts(emailAddress);
                    var userDetail = await _userRepository.SingleOrDefaultAsync(s => s.Email == emailAddress) ?? GenerateUser(firstName: "", lastName: "", emailAddress, authenticationResponse.Identifier, authenticationResponse.Source);
                    var session = await CreateSession(ipAddress, userDetail);
                    Log.Information("Log in granted! User: {0} is logged in.", emailAddress);

                    return new AuthenticationResult()
                    {
                        SessionId = session.SessionId.ToString()
                    };
                }
                else
                {
                    var failedLoginAttempts = await IncreaseFailedLoginAttempts(emailAddress);
                    var loginAttemptLeft = MaxLoginAttempts - failedLoginAttempts;
                    Log.Information(InvalidUsernameOrPasswordError + " Number of attemps left:{0} for user {1}", loginAttemptLeft, emailAddress);

                    return new AuthenticationResult()
                    {
                        ErrorMessage = InvalidUsernameOrPasswordError + " Number of attemps left: " + loginAttemptLeft
                    };
                }
            }

            catch (Exception ex)
            {
                Log.Information(SystemDown);

                return new AuthenticationResult()
                {
                    ErrorMessage = SystemDown
                };
            }

        }

        private async Task<(bool isUserActive, bool isUserRegistered)> IsUserActive(string EmailId)
        {
            bool isUserActive = false;
            bool isUserRegistered = false;
            var userStatusCheck = await _userRepository.SingleOrDefaultAsync(x => x.Email.ToUpper() == EmailId.ToUpper());

            if (userStatusCheck != null)
            {
                isUserActive = userStatusCheck.IsAccountActive;
                isUserRegistered = true;
            }

            return (isUserActive, isUserRegistered);
        }

        private async Task ResetFailedLoginAttempts(string EmailId)
        {
            var UserDetails = await _userRepository.SingleOrDefaultAsync(x => x.Email.ToUpper() == EmailId.ToUpper());
            UserDetails.FailedLoginAttempts = 0;
            await _userRepository.Update(UserDetails);
        }

        private async Task<int> IncreaseFailedLoginAttempts(string EmailId)
        {
            var UserDetails = await _userRepository.SingleOrDefaultAsync(x => x.Email.ToUpper() == EmailId.ToUpper());
            UserDetails.FailedLoginAttempts += 1;

            if (UserDetails.FailedLoginAttempts >= 5)
            {
                UserDetails.IsAccountActive = false;
                UserDetails.AccountInactiveReason = AccountInactiveReason;
            }

            await _userRepository.Update(UserDetails);
            return UserDetails.FailedLoginAttempts;
        }

        private async Task<Session> CreateSession(string ipAddress, User user)
        {
            var session = new Session()
            {
                UserId = user.Id,
                SessionId = Guid.NewGuid(),
                IPAddress = ipAddress,
                ExpireAt = DateTime.UtcNow.AddMinutes(15),
                LastModifiedBy = ""
            };

            await _sessionRepository.Create(session);
            return session;
        }

        public async Task<AuthenticationResult> Logout(string sessionId)
        {
            var session = await _sessionRepository.SingleOrDefaultAsync(s => s.SessionId.ToString() == sessionId);

            if (session == default)
            {
                Log.Information("Session with Id {0} not active :{1}", sessionId, DateTime.Now);
                return new AuthenticationResult()
                {
                    ErrorMessage = LogoutFailed
                };
            }

            session.ExpireAt = DateTime.UtcNow;
            await _sessionRepository.Update(session);
            return new AuthenticationResult();
        }
    }
}