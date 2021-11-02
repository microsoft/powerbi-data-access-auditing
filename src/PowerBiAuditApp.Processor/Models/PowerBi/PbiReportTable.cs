using System;
using System.Linq;
using System.Runtime.Serialization;
using Azure;
using Microsoft.PowerBI.Api.Models;
using Newtonsoft.Json;
using ITableEntity = Azure.Data.Tables.ITableEntity;

namespace PowerBiAuditApp.Processor.Models.PowerBi;

public class PbiReportTable : ITableEntity
{
    public string RowKey { get { return Id.ToString(); } set { Id = new Guid(value); } }
    [IgnoreDataMember]
    public Guid Id { get; set; }

    public string PartitionKey { get { return GroupId.ToString(); } set { GroupId = new Guid(value); } }
    [IgnoreDataMember]
    public Guid GroupId { get; set; }

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
    [IgnoreDataMember]
    public string[] Users {
        get => JsonConvert.DeserializeObject<string[]>(StringUsers);
        set => StringUsers = JsonConvert.SerializeObject(value);
    }

    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

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