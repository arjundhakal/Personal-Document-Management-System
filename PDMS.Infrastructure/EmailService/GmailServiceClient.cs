using PDMS.Application.Interfaces;
using PDMS.Domain.Entity;
using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace PDMS.Infrastructure.EmailService
{
    public class GmailServiceClient : IEmailServiceClient
    {
        private readonly string _emailAddress;
        private readonly string _emailPassword;
        private readonly string _host;
        private readonly int _port;
        private readonly int _timeOut;

        private const string EmailProviderSource = "Gmail";

        public GmailServiceClient(IConfiguration config)
        {
            _emailAddress = config.GetValue<string>("GmailService:EmailAddress");
            _emailPassword = config.GetValue<string>("GmailService:EmailPassword");
            _host = config.GetValue<string>("GmailService:Host");
            _port = config.GetValue<int>("GmailService:Port");
            _timeOut = config.GetValue<int>("GmailService:TimeOut");
        }

        public async Task<EmailServiceResult> SendEmail(string toEmailAddress, string toUserFirstName, string PasswordResetLink)
        {
            var fromAddress = new MailAddress(_emailAddress, "Personal Document management");
            var toAddress = new MailAddress(toEmailAddress, toUserFirstName);
            var fromPassword = _emailPassword;
            const string subject = "Reset Your PDMS Password";
            string body = "Please find the Password Reset Link:  " + PasswordResetLink + ". Please Note this link is valid only for 24 hours!";
            try
            {
                var smtp = new SmtpClient
                {
                    Host = _host,
                    Port = _port,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword),
                    Timeout = _timeOut
                };
                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body
                })
                {
                    smtp.Send(message);
                }

                return new EmailServiceResult()
                {
                    Source = EmailProviderSource,
                    ErrorMessage = null
                };
                Log.Information("Password reset link is sent for {0} in their emailId.", toUserFirstName);
            }
            catch (Exception ex)
            {
                Log.Information("Password reset link could not be sent for {0} in their emailId. {1}", toUserFirstName, ex.Message);
                return new EmailServiceResult()
                {
                    Source = EmailProviderSource,
                    ErrorMessage = ex.Message
                };
            }
        }

    }
}
