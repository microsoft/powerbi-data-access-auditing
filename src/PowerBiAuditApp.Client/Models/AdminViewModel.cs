using System.Collections.Generic;
using PowerBiAuditApp.Models;

namespace PowerBiAuditApp.Client.Models
{
    public class AdminViewModel
    {
        public Dictionary<string, ReportDetail[]> Reports { get; init; } = null!;
    }
}
