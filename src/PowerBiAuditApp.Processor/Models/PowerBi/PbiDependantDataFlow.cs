using Microsoft.PowerBI.Api.Models;

namespace PowerBiAuditApp.Processor.Models.PowerBi
{
    public class PbiDependantDataFlow
    {
        public string TargetDataFlowId { get; set; }
        public string GroupId { get; set; }

        public PbiDependantDataFlow(DependentDataflow dependentDataFlow)
        {
            TargetDataFlowId = dependentDataFlow.TargetDataflowId;
            GroupId = dependentDataFlow.GroupId;
        }
    }
}