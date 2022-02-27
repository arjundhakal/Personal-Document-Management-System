using PDMS.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDMS.Application.Interfaces
{
    public interface IAuthenticationClient
    {
        Task<AuthenticationCreatedUserResult> CreateUser(string username, string password);
        Task<AuthenticationLoginUserResult> Login(string username, string password);
        Task<AuthenticationCreatedUserResult> ChangePassword(string identifier, string newPassword);
    }
}
