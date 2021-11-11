using Microsoft.PowerBI.Api.Models;

namespace PowerBiAuditApp.Services;

public interface IPowerBiReportService
{
    Task<IList<Group>> GetGroups();

    Task<IList<Report>> GetReports(Guid groupId);

    Task<IList<Dataset>> GetDataSets(Guid groupId);
    Task<IList<Dashboard>> GetDashboards(Guid groupId);
    Task<IList<Dataflow>> GetDataFlows(Guid groupId);
    Task<ActivityEventResponse> GetActivityEvents(DateTimeOffset start, DateTimeOffset end);
    Task<ActivityEventResponse?> GetActivityEvents(ActivityEventResponse? activityEventResponse);
}