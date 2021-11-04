using System;
using Azure;
using Azure.Data.Tables;

namespace PowerBiAuditApp.Processor.Models.PowerBi;

public class PbiProcessRunTable : ITableEntity
{
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset LastProcessedDate { get; set; }

    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}