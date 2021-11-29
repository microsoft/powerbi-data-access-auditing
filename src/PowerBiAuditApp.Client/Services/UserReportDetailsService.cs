using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using PowerBiAuditApp.Models;

namespace PowerBiAuditApp.Client.Services
{
    public class UserReportDetailsService : IUserReportDetailsService
    {
        private readonly IReportDetailsService _reportDetailsService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserReportDetailsService(IReportDetailsService reportDetailsService, IHttpContextAccessor httpContextAccessor)
        {
            _reportDetailsService = reportDetailsService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IList<ReportDetail>> GetReportDetails()
        {
            var userGroups = _httpContextAccessor.HttpContext?.User.Claims
                .Where(x => x.Type == "groups")
                .Select(x => new Guid(x.Value))
                .ToArray() ?? Array.Empty<Guid>();

            return (await _reportDetailsService.GetReportDetails()).Where(x => x.AadGroups.Select(a => a.Id).Any(a => x.Enabled && userGroups.Contains(a))).ToList();
        }

        public async Task<ReportDetail> GetReportDetail(Guid workspaceId, Guid reportId) => (await GetReportDetails()).FirstOrDefault(x => x.GroupId == workspaceId && x.ReportId == reportId);
    }
}
