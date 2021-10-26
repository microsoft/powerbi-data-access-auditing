namespace PowerBiAuditApp.Models;

public class HomeViewModel
{
    public string? User { get; init; }
    public IList<ReportDetails> Reports { get; init; } = null!;
}