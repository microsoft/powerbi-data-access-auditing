using System.Collections.Generic;
using PowerBiAuditApp.Models;

namespace PowerBiAuditApp.Client.Models
{
    public class HomeViewModel
    {
        public string User { get; init; }
        public Dictionary<string, ReportDetail[]> Reports { get; init; } = null!;
    }
}