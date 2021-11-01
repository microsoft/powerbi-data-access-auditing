namespace PowerBiAuditApp.Client.Models;

public class ReportDetails
{
    public int? UniqueId { get; set; }

    public int? DisplayLevel { get; set; }

    public string? DisplayName { get; set; }
    // Workspace Id for which Embed token needs to be generated
    public Guid WorkspaceId { get; set; }

    public string? Description { get; set; }

    // Report Id for which Embed token needs to be generated
    public Guid ReportId { get; set; }
    public string[] Roles { get; set; } = Array.Empty<string>();

    public List<int>? DrillThroughReports { get; set; }

    public List<string>? RequiredParameters { get; set; }

    public string? PaginationTable { get; set; }

    public string? PaginationColumn { get; set; }
}