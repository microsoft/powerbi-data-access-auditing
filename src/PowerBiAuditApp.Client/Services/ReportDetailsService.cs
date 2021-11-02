﻿using Microsoft.Extensions.Options;
using PowerBiAuditApp.Client.Models;

namespace PowerBiAuditApp.Client.Services;

public class ReportDetailsService : IReportDetailsService
{
    private readonly IOptions<List<ReportDetails>> _reportDetails;

    public ReportDetailsService(IOptions<List<ReportDetails>> reportDetails)
    {
        _reportDetails = reportDetails;
    }

    public IList<ReportDetails> GetReportDetails() => _reportDetails.Value;

    public ReportDetails? GetReportDetails(Guid workspaceId, Guid reportId) => _reportDetails.Value.FirstOrDefault(x => x.WorkspaceId == workspaceId && x.ReportId == reportId);
}