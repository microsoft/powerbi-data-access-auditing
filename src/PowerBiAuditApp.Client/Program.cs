using Azure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Azure;
using Microsoft.Identity.Web;
using PowerBiAuditApp.Client.Middleware;
using PowerBiAuditApp.Client.Models;
using PowerBiAuditApp.Client.Services;
using PowerBiAuditApp.Services;
using PowerBiAuditApp.Services.Models;

var builder = WebApplication.CreateBuilder(args);

// Register Services
builder.Services.Configure<ServicePrincipal>(builder.Configuration.GetSection("ServicePrincipal"));
builder.Services.Configure<StorageAccountSettings>(builder.Configuration.GetSection("StorageAccountSettings"));
//builder.Services.Configure<List<ReportDetails>>(builder.Configuration.GetSection("ReportDetails"));
builder.Services.AddScoped<IAuditLogger, AuditLogger>();
builder.Services.AddScoped<IPowerBiTokenProvider, PowerBiTokenProvider>();
builder.Services.AddScoped<IReportDetailsService, ReportDetailsService>();
builder.Services.AddScoped<IPowerBiEmbeddedReportService, PowerBiEmbeddedReportService>();
builder.Services.AddMemoryCache();

builder.Services.AddDataProtection();

builder.Services.AddAzureClients(b =>
{
    b.UseCredential(new DefaultAzureCredential());
    b.AddTableServiceClient(builder.Configuration.GetSection("StorageAccountSettings").GetSection("TableEndpoint"));
    b.AddBlobServiceClient(builder.Configuration.GetSection("StorageAccountSettings").GetSection("BlobEndpoint"));
    b.AddQueueServiceClient(builder.Configuration.GetSection("StorageAccountSettings").GetSection("QueueEndpoint"))
        .ConfigureOptions(c => c.MessageEncoding = Azure.Storage.Queues.QueueMessageEncoding.Base64);
});


builder.Services.AddMicrosoftIdentityWebAppAuthentication(builder.Configuration);

builder.Services.AddControllersWithViews();

builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.UseMiddleware<PowerBiReverseProxyMiddleware>();

app.Run();
