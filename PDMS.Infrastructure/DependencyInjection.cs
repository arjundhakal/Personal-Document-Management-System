using PDMS.Application.Interfaces;
using PDMS.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PDMS.Infrastructure.Authentication;
using PDMS.Infrastructure.EmailService;

namespace PDMS.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuation)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                                                        options.UseSqlServer(configuation.GetConnectionString("DefaultConnection"),
                                                        b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

            services.AddScoped<IApplicationDbContext>(p => p.GetService<ApplicationDbContext>());
            services.AddScoped<IAuthenticationClient, AuthZeroAuthenticationClient>();
            services.AddScoped<IDocumentStorageClient, NimbusDocumentStorageClient>();
            services.AddScoped<IEmailServiceClient, GmailServiceClient>();
            return services;
        }
    }
}