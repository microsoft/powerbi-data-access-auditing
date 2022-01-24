using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace PowerBiAuditApp.Client.Services
{
    public interface IAuditLogger
    {
        Task CreateAuditLog(HttpContext httpContext, HttpResponseMessage responseMessage, Guid? reportId, string reportName, CancellationToken cancellationToken = default);
    }
}