using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;
using PowerBiAuditApp.Middleware;
using PowerBiAuditApp.Models;
using PowerBiAuditApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Register Services
builder.Services.Configure<ServicePrincipal>(builder.Configuration.GetSection("ServicePrincipal"));
builder.Services.Configure<List<ReportDetails>>(builder.Configuration.GetSection("ReportDetails"));
builder.Services.AddScoped<IPowerBiTokenProvider, PowerBiTokenProvider>();
builder.Services.AddScoped<IReportDetailsService, ReportDetailsService>();
builder.Services.AddScoped<IPowerBiReportService, PowerBiReportService>();


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
