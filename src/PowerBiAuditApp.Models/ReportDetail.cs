using System.Runtime.Serialization;
using Azure;
using Azure.Data.Tables;
using Newtonsoft.Json;

namespace PowerBiAuditApp.Models;

public class ReportDetail : ITableEntity
{
    public string RowKey { get { return ReportId.ToString(); } set { ReportId = new Guid(value); } }
    // Report Id for which Embed token needs to be generated
    [IgnoreDataMember]
    public Guid ReportId { get; set; }
    public string PartitionKey { get { return GroupId.ToString(); } set { GroupId = new Guid(value); } }

    // Workspace Id for which Embed token needs to be generated
    [IgnoreDataMember]
    public Guid GroupId { get; set; }

    public bool Enabled { get; set; }

    public int? DisplayLevel { get; set; }

    public string? Name { get; set; }
    public string? GroupName { get; set; }

    public string? Description { get; set; }

    public string? ReportType { get; set; }

    public bool Deleted { get; set; }

    public string StringRoles { get; set; } = null!;

    [IgnoreDataMember]
    public string[] Roles {
        get => JsonConvert.DeserializeObject<string[]>(StringRoles);
        set => StringRoles = JsonConvert.SerializeObject(value);
    }

    public List<int>? DrillThroughReports { get; set; }

    public List<string>? RequiredParameters { get; set; }

    public string? PaginationTable { get; set; }

    public string? PaginationColumn { get; set; }

    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}