using System;
using System.Linq;
using Microsoft.PowerBI.Api.Models;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace PowerBiAuditApp.Processor.Models.PowerBi
{
    public class PbiDataFlowTable : TableEntity
    {
        [IgnoreProperty]
        public Guid ObjectId { get { return new Guid(RowKey); } set { RowKey = value.ToString(); } }

        [IgnoreProperty]
        public Guid GroupId { get { return new Guid(PartitionKey); } set { PartitionKey = value.ToString(); } }

        public string Name { get; set; }
        public string Description { get; set; }
        public string ModelUrl { get; set; }
        public string ConfiguredBy { get; set; }
        public string ModifiedBy { get; set; }
        public string EndorsementDetailsCertifiedBy { get; set; }
        public string EndorsementDetailsEndorsement { get; set; }
        public DateTime? ModifiedDateTime { get; set; }
        public string StringDataSourceUsageInstanceIds { get; set; }
        [IgnoreProperty]
        public Guid[] DataSourceUsageInstanceIds {
            get => JsonConvert.DeserializeObject<Guid[]>(StringDataSourceUsageInstanceIds);
            set => StringDataSourceUsageInstanceIds = JsonConvert.SerializeObject(value);
        }
        public string StringUpstreamDataFlows { get; set; }
        [IgnoreProperty]
        public PbiDependantDataFlow[] UpstreamDataFlows {
            get => JsonConvert.DeserializeObject<PbiDependantDataFlow[]>(StringUpstreamDataFlows);
            set => StringUpstreamDataFlows = JsonConvert.SerializeObject(value);
        }
        public Guid? SensitivityLabelId { get; set; }
        public string StringUsers { get; set; }
        [IgnoreProperty]
        public string[] Users {
            get => JsonConvert.DeserializeObject<string[]>(StringUsers);
            set => StringUsers = JsonConvert.SerializeObject(value);
        }

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
}