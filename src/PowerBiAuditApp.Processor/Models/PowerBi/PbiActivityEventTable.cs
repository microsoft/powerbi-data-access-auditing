using System;
using System.Runtime.Serialization;
using Azure;
using Azure.Data.Tables;

namespace PowerBiAuditApp.Processor.Models.PowerBi;

public class PbiActivityEventTable : ITableEntity
{
    public string RowKey { get { return Id.ToString(); } set { Id = new Guid(value); } }
    [IgnoreDataMember]
    public Guid Id { get; set; }

    public string PartitionKey { get { return GroupId.ToString(); } set { GroupId = new Guid(value); } }
    [IgnoreDataMember]
    public Guid GroupId { get; set; }


    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}