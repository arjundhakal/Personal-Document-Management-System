using PDMS.Application.Interfaces;
using PDMS.Application.Repositories;
using Microsoft.Extensions.DependencyInjection;
using PDMS.Application.Services;

namespace PDMS.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped(typeof(IDatabaseRepository<>), typeof(DatabaseRepository<>));
            services.AddScoped<ISessionService, SessionService>();
            services.AddScoped<ISettingsService, SettingsService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IDocumentManagementService, DocumentManagementService>();
            services.AddScoped<IResetPasswordService, ResetPasswordService>();
            return services;
        }
    }
}