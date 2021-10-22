namespace PowerBiAuditApp.Models;

public class ReportViewModel
{
    public string? User { get; set; }
    public string EmbedToken { get; init; } = null!;
    public string EmbedUrl { get; init; } = null!;
    public Guid ReportId { get; set; }
    public Guid WorkspaceId { get; set; }
    public int PageNumber { get; set; }
    public string? PaginationTable { get; set; }
}