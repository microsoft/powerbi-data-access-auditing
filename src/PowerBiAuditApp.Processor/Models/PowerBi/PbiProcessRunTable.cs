using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace PowerBiAuditApp.Processor.Models.PowerBi
{
    public class PbiProcessRunTable : TableEntity
    {
        public DateTimeOffset LastProcessedDate { get; set; }
    }
}