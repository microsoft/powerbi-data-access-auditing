using System;
using System.Linq;
using Microsoft.PowerBI.Api.Models;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace PowerBiAuditApp.Processor.Models.PowerBi
{
    public class PbiReportTable : TableEntity
    {
        [IgnoreProperty]
        public Guid Id { get { return new Guid(RowKey); } set { RowKey = value.ToString(); } }

        [IgnoreProperty]
        public Guid GroupId { get { return new Guid(PartitionKey); } set { PartitionKey = value.ToString(); } }

        public string Name { get; set; }

        public string WebUrl { get; set; }

        public string EmbedUrl { get; set; }

        public string DatasetId { get; set; }

        public string Description { get; set; }

        public string CreatedBy { get; set; }

        public string ModifiedBy { get; set; }

        public DateTime? CreatedDateTime { get; set; }

        public DateTime? ModifiedDateTime { get; set; }

        public string EndorsementDetailsCertifiedBy { get; set; }
        public string EndorsementDetailsEndorsement { get; set; }

        public Guid? SensitivityLabelId { get; set; }

        public string ReportType { get; set; }

        public string StringUsers { get; set; }
        [IgnoreProperty]
        public string[] Users {
            get => JsonConvert.DeserializeObject<string[]>(StringUsers);
            set => StringUsers = JsonConvert.SerializeObject(value);
        }

        public PbiReportTable() { }

        public PbiReportTable(Report report, Guid groupId)
        {
            Id = report.Id;
            GroupId = groupId;
            Name = report.Name;
            WebUrl = report.WebUrl;
            EmbedUrl = report.EmbedUrl;
            DatasetId = report.DatasetId;
            Description = report.Description;
            CreatedBy = report.CreatedBy;
            ModifiedBy = report.ModifiedBy;
            CreatedDateTime = report.CreatedDateTime;
            ModifiedDateTime = report.ModifiedDateTime;
            EndorsementDetailsCertifiedBy = report.EndorsementDetails?.CertifiedBy;
            EndorsementDetailsEndorsement = report.EndorsementDetails?.Endorsement;
            SensitivityLabelId = report.SensitivityLabel?.LabelId;
            ReportType = report.ReportType;
            Users = report.Users?.Select(x => x.Identifier).ToArray() ?? Array.Empty<string>();
        }
    }
}