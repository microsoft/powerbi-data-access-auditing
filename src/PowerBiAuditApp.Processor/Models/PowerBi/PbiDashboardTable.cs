using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Azure;
using Azure.Data.Tables;
using Microsoft.PowerBI.Api.Models;
using Newtonsoft.Json;

namespace PowerBiAuditApp.Processor.Models.PowerBi;

public class PbiDashboardTable : ITableEntity
{
    public string RowKey { get { return Id.ToString(); } set { Id = new Guid(value); } }
    [IgnoreDataMember]
    public Guid Id { get; set; }
    public string PartitionKey { get { return GroupId.ToString(); } set { GroupId = new Guid(value); } }
    [IgnoreDataMember]
    public Guid GroupId { get; set; }

    public string DisplayName { get; set; }
    public bool? IsReadOnly { get; set; }
    public string EmbedUrl { get; set; }
    public IList<Tile> Tiles { get; set; }
    public string DataClassification { get; set; }
    public Guid? SensitivityLabelId { get; set; }
    public string StringUsers { get; set; }
    [IgnoreDataMember]
    public string[] Users {
        get => JsonConvert.DeserializeObject<string[]>(StringUsers);
        set => StringUsers = JsonConvert.SerializeObject(value);
    }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }


    public PbiDashboardTable() { }

    public PbiDashboardTable(Dashboard dashboard, Guid groupId)
    {
        Id = dashboard.Id;
        GroupId = groupId;
        DisplayName = dashboard.DisplayName;
        IsReadOnly = dashboard.IsReadOnly;
        EmbedUrl = dashboard.EmbedUrl;
        Tiles = dashboard.Tiles;
        DataClassification = dashboard.DataClassification;
        SensitivityLabelId = dashboard.SensitivityLabel?.LabelId;
        Users = dashboard.Users.Select(x => x.Identifier).ToArray();
    }
}