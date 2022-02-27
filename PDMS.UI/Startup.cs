using PDMS.Application;
using PDMS.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using Serilog;

namespace PDMS.UI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            InitialiseLogger();
            Log.Information("PDMS is starting up.");
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddInfrastructure(Configuration);
            services.AddApplication();
            services.AddControllersWithViews();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(15);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
            services.AddHttpContextAccessor();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseSession();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Login}/{action=Index}/{id?}");
            });
        }

        private void InitialiseLogger()
        {
                var executableLocation = Assembly.GetEntryAssembly().Location;
                var executablePath = Path.GetDirectoryName(executableLocation);

                var logTemplate =
                    "[{MachineName}] {Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}";
                var logFileName = "PDMS" + "_log.txt";
                var filePath = Configuration.GetValue<string>("Logging:Path");

                var logger = new LoggerConfiguration()
                    .Enrich.WithMachineName()
                    .MinimumLevel.Debug()
                    .WriteTo.Console()
                    .WriteTo.File(string.IsNullOrEmpty(filePath) ? Path.Combine(executablePath, "logs", logFileName) : Path.Combine(filePath, logFileName),
                        rollOnFileSizeLimit: true,
                        rollingInterval: RollingInterval.Day,
                        outputTemplate: logTemplate
                    );

                if (Configuration.GetValue<bool>("Logging:EnableDebugLogging"))
                {
                    logger = logger
                        .MinimumLevel.Debug()
                        .WriteTo.Console();
                }
                else
                {
                    logger = logger.MinimumLevel.Information();
                }

                Log.Logger = logger.CreateLogger();
        }
    }
}