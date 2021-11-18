using System.Collections.Generic;
using PowerBiAuditApp.Models;

namespace PowerBiAuditApp.Client.Models
{
    public class AdminViewModel
    {
        public IList<ReportDetail> Reports { get; init; } = null!;
    }
}
