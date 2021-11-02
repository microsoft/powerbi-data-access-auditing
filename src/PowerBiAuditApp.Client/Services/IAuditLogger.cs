namespace PowerBiAuditApp.Client.Services;

public interface IAuditLogger
{
    Task CreateAuditLog(HttpContext httpContext, HttpResponseMessage responseMessage, CancellationToken cancellationToken = default);
}