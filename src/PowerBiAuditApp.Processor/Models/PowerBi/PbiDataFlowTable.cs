using System;
using System.Linq;
using System.Runtime.Serialization;
using Azure;
using Azure.Data.Tables;
using Microsoft.PowerBI.Api.Models;
using Newtonsoft.Json;

namespace PowerBiAuditApp.Processor.Models.PowerBi;

public class PbiDataFlowTable : ITableEntity
{

    public string RowKey { get { return ObjectId.ToString(); } set { ObjectId = new Guid(value); } }
    [IgnoreDataMember]
    public Guid ObjectId { get; set; }

    public string PartitionKey { get { return GroupId.ToString(); } set { GroupId = new Guid(value); } }
    [IgnoreDataMember]
    public Guid GroupId { get; set; }

    public string Name { get; set; }
    public string Description { get; set; }
    public string ModelUrl { get; set; }
    public string ConfiguredBy { get; set; }
    public string ModifiedBy { get; set; }
    public string EndorsementDetailsCertifiedBy { get; set; }
    public string EndorsementDetailsEndorsement { get; set; }
    public DateTime? ModifiedDateTime { get; set; }
    public string StringDataSourceUsageInstanceIds { get; set; }
    [IgnoreDataMember]
    public Guid[] DataSourceUsageInstanceIds {
        get => JsonConvert.DeserializeObject<Guid[]>(StringDataSourceUsageInstanceIds);
        set => StringDataSourceUsageInstanceIds = JsonConvert.SerializeObject(value);
    }
    public string StringUpstreamDataFlows { get; set; }
    [IgnoreDataMember]
    public PbiDependantDataFlow[] UpstreamDataFlows {
        get => JsonConvert.DeserializeObject<PbiDependantDataFlow[]>(StringUpstreamDataFlows);
        set => StringUpstreamDataFlows = JsonConvert.SerializeObject(value);
    }
    public Guid? SensitivityLabelId { get; set; }
    public string StringUsers { get; set; }
    [IgnoreDataMember]
    public string[] Users {
        get => JsonConvert.DeserializeObject<string[]>(StringUsers);
        set => StringUsers = JsonConvert.SerializeObject(value);
    }


    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }


    public PbiDataFlowTable() { }
    public PbiDataFlowTable(Dataflow dataFlow, Guid groupId)
    {
        ObjectId = dataFlow.ObjectId;
        GroupId = groupId;
        Name = dataFlow.Name;
        Description = dataFlow.Description;
        ModelUrl = dataFlow.ModelUrl;
        ConfiguredBy = dataFlow.ConfiguredBy;
        ModifiedBy = dataFlow.ModifiedBy;
        EndorsementDetailsCertifiedBy = dataFlow.EndorsementDetails?.CertifiedBy;
        EndorsementDetailsEndorsement = dataFlow.EndorsementDetails?.Endorsement;
        ModifiedDateTime = dataFlow.ModifiedDateTime;
        DataSourceUsageInstanceIds = dataFlow.DatasourceUsages?.Select(x => x.DatasourceInstanceId).ToArray() ?? Array.Empty<Guid>();
        UpstreamDataFlows = dataFlow.UpstreamDataflows?.Select(x => new PbiDependantDataFlow(x)).ToArray() ?? Array.Empty<PbiDependantDataFlow>();
        SensitivityLabelId = dataFlow.SensitivityLabel?.LabelId;
        Users = dataFlow.Users.Select(x => x.Identifier).ToArray();
    }
}