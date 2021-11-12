using System;
using System.Linq;
using Microsoft.PowerBI.Api.Models;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace PowerBiAuditApp.Processor.Models.PowerBi
{
    public class PbiDataSetTable : TableEntity
    {
        [IgnoreProperty]
        public string Id { get { return RowKey; } set { RowKey = value; } }

        [IgnoreProperty]
        public Guid GroupId { get { return new Guid(PartitionKey); } set { PartitionKey = value.ToString(); } }

        public string Name { get; set; }
        public string ConfiguredBy { get; set; }
        public bool? AddRowsApiEnabled { get; set; }
        public string WebUrl { get; set; }
        public bool? IsRefreshable { get; set; }
        public bool? IsEffectiveIdentityRequired { get; set; }
        public bool? IsEffectiveIdentityRolesRequired { get; set; }
        public bool? IsOnPremGatewayRequired { get; set; }
        //public Encryption Encryption { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string ContentProviderType { get; set; }
        public string CreateReportEmbedUrl { get; set; }
        public string QnaEmbedUrl { get; set; }
        public string Description { get; set; }
        public string EndorsementDetailsCertifiedBy { get; set; }
        public string EndorsementDetailsEndorsement { get; set; }

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
        //public IList<Table> Tables { get; set; }
        public Guid? SensitivityLabelId { get; set; }

        public string StringUsers { get; set; }
        [IgnoreProperty]
        public string[] Users {
            get => JsonConvert.DeserializeObject<string[]>(StringUsers);
            set => StringUsers = JsonConvert.SerializeObject(value);
        }

        public string SchemaRetrievalError { get; set; }
        public bool? SchemaMayNotBeUpToDate { get; set; }


        public PbiDataSetTable() { }

        public PbiDataSetTable(Dataset dataSet, Guid groupId)
        {
            Id = dataSet.Id;
            GroupId = groupId;
            Name = dataSet.Name;
            ConfiguredBy = dataSet.ConfiguredBy;
            AddRowsApiEnabled = dataSet.AddRowsAPIEnabled;
            WebUrl = dataSet.WebUrl;
            IsRefreshable = dataSet.IsRefreshable;
            IsEffectiveIdentityRequired = dataSet.IsEffectiveIdentityRequired;
            IsEffectiveIdentityRolesRequired = dataSet.IsEffectiveIdentityRolesRequired;
            IsOnPremGatewayRequired = dataSet.IsOnPremGatewayRequired;
            //Encryption = dataSet.Encryption;
            CreatedDate = dataSet.CreatedDate;
            ContentProviderType = dataSet.ContentProviderType;
            CreateReportEmbedUrl = dataSet.CreateReportEmbedURL;
            QnaEmbedUrl = dataSet.QnaEmbedURL;
            Description = dataSet.Description;
            EndorsementDetailsCertifiedBy = dataSet.EndorsementDetails?.CertifiedBy;
            EndorsementDetailsEndorsement = dataSet.EndorsementDetails?.Endorsement;
            DataSourceUsageInstanceIds = dataSet.DatasourceUsages?.Select(x => x.DatasourceInstanceId).ToArray() ?? Array.Empty<Guid>();
            UpstreamDataFlows = dataSet.UpstreamDataflows?.Select(x => new PbiDependantDataFlow(x)).ToArray() ?? Array.Empty<PbiDependantDataFlow>();
            //Tables = dataSet.Tables;
            SensitivityLabelId = dataSet.SensitivityLabel?.LabelId;
            Users = dataSet.Users?.Select(x => x.Identifier).ToArray() ?? Array.Empty<string>();
            SchemaRetrievalError = dataSet.SchemaRetrievalError;
            SchemaMayNotBeUpToDate = dataSet.SchemaMayNotBeUpToDate;
        }
    }
}