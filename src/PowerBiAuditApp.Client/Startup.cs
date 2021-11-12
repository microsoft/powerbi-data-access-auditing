using System;
using Azure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using PowerBiAuditApp.Client.Middleware;
using PowerBiAuditApp.Client.Models;
using PowerBiAuditApp.Client.Services;
using PowerBiAuditApp.Services;
using PowerBiAuditApp.Services.Models;

namespace PowerBiAuditApp.Client
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Register Services
            services.Configure<ServicePrincipal>(Configuration.GetSection("ServicePrincipal"));
            services.Configure<StorageAccountSettings>(Configuration.GetSection("StorageAccountSettings"));

            services.AddScoped<IAuditLogger, AuditLogger>();
            services.AddScoped<IPowerBiTokenProvider, PowerBiTokenProvider>();
            services.AddScoped<IReportDetailsService, ReportDetailsService>();
            services.AddScoped<IPowerBiEmbeddedReportService, PowerBiEmbeddedReportService>();
            services.AddMemoryCache();

            services.AddDataProtection();
            services.AddHttpContextAccessor();

            services.AddAzureClients(clientFactory =>
            {
                var storageAccountSettings = Configuration.GetSection("StorageAccountSettings");

                clientFactory.UseCredential(new DefaultAzureCredential());

                var tableEndpoint = storageAccountSettings.GetSection("TableEndpoint");
                if (Uri.TryCreate(tableEndpoint.Value, UriKind.Absolute, out var tableUri))
                    clientFactory.AddTableServiceClient(tableUri);
                else
                    clientFactory.AddTableServiceClient(tableEndpoint.Value);

                var blobEndpoint = storageAccountSettings.GetSection("BlobEndpoint");
                if (Uri.TryCreate(blobEndpoint.Value, UriKind.Absolute, out var blobUri))
                    clientFactory.AddBlobServiceClient(blobUri);
                else
                    clientFactory.AddBlobServiceClient(blobEndpoint.Value);

                var queueEndpoint = storageAccountSettings.GetSection("QueueEndpoint");
                if (Uri.TryCreate(queueEndpoint.Value, UriKind.Absolute, out var queueUri))
                    clientFactory.AddQueueServiceClient(queueUri)
                        .ConfigureOptions(c => c.MessageEncoding = Azure.Storage.Queues.QueueMessageEncoding.Base64);
                else
                    clientFactory.AddQueueServiceClient(queueEndpoint.Value)
                        .ConfigureOptions(c => c.MessageEncoding = Azure.Storage.Queues.QueueMessageEncoding.Base64);
            });


            services.AddMicrosoftIdentityWebAppAuthentication(Configuration);

            services.AddControllersWithViews();

            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();

                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            // Configure the HTTP request pipeline.
            if (!env.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(e => e.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}"
                )
            );
            app.UseMiddleware<PowerBiReverseProxyMiddleware>();

        }
    }
}
