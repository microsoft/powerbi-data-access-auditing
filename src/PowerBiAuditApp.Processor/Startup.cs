using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PowerBiAuditApp.Services;
using PowerBiAuditApp.Services.Models;

[assembly: FunctionsStartup(typeof(PowerBiAuditApp.Processor.Startup))]

namespace PowerBiAuditApp.Processor
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var configuration = builder.Services.BuildServiceProvider().GetRequiredService<IConfiguration>();

            builder.Services.AddOptions<ServicePrincipal>().Configure<IConfiguration>((settings, conf) =>
            {
                conf.GetSection("ServicePrincipal").Bind(settings);
            });
            builder.Services.AddScoped<IPowerBiTokenProvider, PowerBiTokenProvider>();
            builder.Services.AddScoped<IPowerBiReportService, PowerBiReportService>();
        }
    }
}