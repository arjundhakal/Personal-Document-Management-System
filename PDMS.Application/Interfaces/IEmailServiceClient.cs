using PDMS.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDMS.Application.Interfaces
{
    public interface IEmailServiceClient
    {
        Task<EmailServiceResult> SendEmail(string toEmailAddress, string toUserFirstName, string PasswordResetLink);
    }
}
