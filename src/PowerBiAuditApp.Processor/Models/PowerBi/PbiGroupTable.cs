using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.PowerBI.Api.Models;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace PowerBiAuditApp.Processor.Models.PowerBi
{
    public class PbiGroupTable : TableEntity
    {

        [IgnoreProperty]
        public Guid Id { get { return new Guid(RowKey); } set { RowKey = value.ToString(); PartitionKey = value.ToString(); } }
        public string Name { get; set; }
        public bool? IsReadOnly { get; set; }
        public bool? IsOnDedicatedCapacity { get; set; }
        public Guid? CapacityId { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string State { get; set; }
        public string StringUsers { get; set; }
        [IgnoreProperty]
        public string[] Users {
            get => JsonConvert.DeserializeObject<string[]>(StringUsers);
            set => StringUsers = JsonConvert.SerializeObject(value);
        }
        public Guid? DataFlowStorageId { get; set; }
        public IList<Workbook> Workbooks { get; set; }
        public Guid? PipelineId { get; set; }



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
}