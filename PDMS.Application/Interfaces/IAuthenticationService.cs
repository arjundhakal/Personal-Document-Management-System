using PDMS.Domain.Entity;
using PDMS.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDMS.Application.Interfaces
{
    public interface IAuthenticationService
    {
        Task<AuthenticationResult> Login(string emailAddress, string password, string ipAddress);
        Task<AuthenticationResult> CreateUser(string firstName, string lastName, string emailAddress, string password);
        Task<AuthenticationResult> Logout(string sessionId);
    }
}