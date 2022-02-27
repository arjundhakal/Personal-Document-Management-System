using Auth0.AuthenticationApi;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using PDMS.Application.Interfaces;
using PDMS.Domain.Entity;
using RestSharp;
using Newtonsoft.Json;
using Serilog;

namespace PDMS.Infrastructure.Authentication
{
    public class AuthZeroAuthenticationClient : IAuthenticationClient
    {
        private const string ConnectionName = "Username-Password-Authentication";
        private const string AuthProviderSource = "auth0";
        private const string GrantType = "client_credentials";

        private readonly AuthenticationApiClient _client;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _clientIdManagementApi;
        private readonly string _clientSecretManagementApi;
        private readonly string _clientAddressManagementApi;

        public AuthZeroAuthenticationClient(IConfiguration config)
        {
            _clientId = config.GetValue<string>("Auth0:ClientId");
            _clientSecret = config.GetValue<string>("Auth0:ClientSecret");
            _client = new AuthenticationApiClient(new Uri($"https://{config.GetValue<string>("Auth0:Domain")}"));

            _clientIdManagementApi = config.GetValue<string>("Auth0ManagementApi:ClientId");
            _clientSecretManagementApi = config.GetValue<string>("Auth0ManagementApi:ClientSecret");
            _clientAddressManagementApi = "https://" + config.GetValue<string>("Auth0ManagementApi:Domain");
        }

        public async Task<AuthenticationCreatedUserResult> CreateUser(string username, string password)
        {
            try
            {
                var result = await _client.SignupUserAsync(new Auth0.AuthenticationApi.Models.SignupUserRequest()
                {
                    ClientId = _clientId,
                    Connection = ConnectionName,
                    Username = username,
                    Password = password,
                    Email = username
                });

                Log.Information("New User {0} registration Successful.", username);
                return new AuthenticationCreatedUserResult()
                {
                    Source = AuthProviderSource,
                    Identifier = result.Id,
                    ErrorMessage = null
                };
            }
            catch (Exception ex)
            {
                Log.Information("New User {0} registration UnSuccessful.", username);
                return new AuthenticationCreatedUserResult()
                {
                    ErrorMessage = ex.Message,
                    Source = AuthProviderSource,
                    Identifier = null
                };
            }
        }

        public async Task<AuthenticationLoginUserResult> Login(string username, string password)
        {
            try
            {
                var result = await _client.GetTokenAsync(new Auth0.AuthenticationApi.Models.ResourceOwnerTokenRequest
                {
                    ClientId = _clientId,
                    ClientSecret = _clientSecret,
                    Scope = "openid profile",
                    Realm = ConnectionName,
                    Username = username,
                    Password = password

                });

                var user = await _client.GetUserInfoAsync(result.AccessToken);
                Log.Information("User {0} log in granted.", username);
                return new AuthenticationLoginUserResult()
                {
                    Source = AuthProviderSource,
                    Identifier = user.UserId,
                    ErrorMessage = null
                };
            }
            catch (Exception ex)
            {
                Log.Information("User {0} login Denied.", username);
                return new AuthenticationLoginUserResult()
                {
                    ErrorMessage = ex.Message,
                    Source = AuthProviderSource,
                    Identifier = null
                };
            }
        }

        public async Task<AuthenticationCreatedUserResult> ChangePassword(string identifier, string newPassword)
        {
            try
            {
                var client1 = new RestClient(_clientAddressManagementApi + "/oauth/token");
                var request1 = new RestRequest(Method.POST);
                request1.AddHeader("content-type", "application/x-www-form-urlencoded");
                request1.AddParameter("application/json", "{ \"grant_type\":" + "\"" + GrantType + "\"" + ",\"client_id\":" + "\"" + _clientIdManagementApi + "\"" + ",\"client_secret\":" + "\"" + _clientSecretManagementApi + "\"" + ",\"audience\":" + "\"" + _clientAddressManagementApi + "/api/v2/\"}", ParameterType.RequestBody);
                IRestResponse response1 = client1.Execute(request1);
                var myDeserializedResponse = JsonConvert.DeserializeObject<dynamic>(response1.Content);
                var token = myDeserializedResponse.access_token;

                Log.Information("New access token is created to change Password for {0}.", identifier);

                var client = new RestClient(_clientAddressManagementApi + "/api/v2/users/auth0%7C" + identifier);
                var request = new RestRequest(Method.PATCH);
                request.AddHeader("content-type", "application/json");
                request.AddHeader("authorization", "Bearer " + token);
                request.AddHeader("cache-control", "no-cache");
                request.AddParameter("application/json", "{ \"password\":" + "\"" + newPassword + "\"" + "}", ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);

                Log.Information("New password has been successfully Changed for {0}.", identifier);
                return new AuthenticationCreatedUserResult()
                {
                    Source = AuthProviderSource,
                    Identifier = null,
                    ErrorMessage = null
                };
            }
            catch (Exception ex)
            {
                Log.Information("Password could not be changed as per request for {0}.", identifier);
                return new AuthenticationCreatedUserResult()
                {
                    ErrorMessage = ex.Message,
                    Source = AuthProviderSource,
                    Identifier = null
                };
            }
        }
    }
}
