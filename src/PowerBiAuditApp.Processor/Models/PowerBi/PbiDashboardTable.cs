using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.PowerBI.Api.Models;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace PowerBiAuditApp.Processor.Models.PowerBi
{
    public class PbiDashboardTable : TableEntity
    {
        [IgnoreProperty]
        public Guid Id { get { return new Guid(RowKey); } set { RowKey = value.ToString(); } }

        [IgnoreProperty]
        public Guid GroupId { get { return new Guid(PartitionKey); } set { PartitionKey = value.ToString(); } }

        public string DisplayName { get; set; }
        public bool? IsReadOnly { get; set; }
        public string EmbedUrl { get; set; }
        public IList<Tile> Tiles { get; set; }
        public string DataClassification { get; set; }
        public Guid? SensitivityLabelId { get; set; }
        public string StringUsers { get; set; }
        [IgnoreProperty]
        public string[] Users {
            get => JsonConvert.DeserializeObject<string[]>(StringUsers);
            set => StringUsers = JsonConvert.SerializeObject(value);
        }


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
}