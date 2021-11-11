using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Azure;
using Azure.Data.Tables;
using Microsoft.PowerBI.Api.Models;
using Newtonsoft.Json;

namespace PowerBiAuditApp.Processor.Models.PowerBi;

public class PbiGroupTable : ITableEntity
{
    public string RowKey { get { return Id.ToString(); } set { Id = new Guid(value); } }
    [IgnoreDataMember]
    public Guid Id { get; set; }
    public string PartitionKey { get { return Id.ToString(); } set { Id = new Guid(value); } }
    public string Name { get; set; }
    public bool? IsReadOnly { get; set; }
    public bool? IsOnDedicatedCapacity { get; set; }
    public Guid? CapacityId { get; set; }
    public string Description { get; set; }
    public string Type { get; set; }
    public string State { get; set; }
    public string StringUsers { get; set; }
    [IgnoreDataMember]
    public string[] Users {
        get => JsonConvert.DeserializeObject<string[]>(StringUsers);
        set => StringUsers = JsonConvert.SerializeObject(value);
    }
    public Guid? DataFlowStorageId { get; set; }
    public IList<Workbook> Workbooks { get; set; }
    public Guid? PipelineId { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }



    public PbiGroupTable() { }

    public PbiGroupTable(Group group)
    {
        Id = group.Id;
        Name = group.Name;
        IsReadOnly = group.IsReadOnly;
        IsOnDedicatedCapacity = group.IsOnDedicatedCapacity;
        CapacityId = group.CapacityId;
        Description = group.Description;
        Type = group.Type;
        State = group.State; Users = group.Users?.Select(x => x.Identifier).ToArray() ?? Array.Empty<string>();
        DataFlowStorageId = group.DataflowStorageId;
        Workbooks = group.Workbooks;
        PipelineId = group.PipelineId;
    }
}