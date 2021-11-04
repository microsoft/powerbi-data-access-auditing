using PowerBiAuditApp.Models;

namespace PowerBiAuditApp.Client.Models;

public class HomeViewModel
{
    public string? User { get; init; }
    public IList<ReportDetail> Reports { get; init; } = null!;
}