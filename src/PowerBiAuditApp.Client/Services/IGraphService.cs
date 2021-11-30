using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PowerBiAuditApp.Models;

namespace PowerBiAuditApp.Client.Services
{
    public interface IGraphService
    {
        Task<List<AadGroup>> GetGroupIds(params string[] groupNames);
        Task<List<AadGroup>> QueryGroups(string groupName);
        Task EnsureRequiredScopes();
    }
}