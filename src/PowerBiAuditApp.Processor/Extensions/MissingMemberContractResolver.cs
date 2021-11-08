using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace PowerBiAuditApp.Processor.Extensions;

public class MissingMemberContractResolver : DefaultContractResolver
{
    protected override JsonObjectContract CreateObjectContract(Type objectType)
    {
        var contract = base.CreateObjectContract(objectType);
        if (contract.ExtensionDataSetter != null && contract.MissingMemberHandling == null)
        {
            contract.MissingMemberHandling = MissingMemberHandling.Ignore;
        }
        return contract;
    }
}