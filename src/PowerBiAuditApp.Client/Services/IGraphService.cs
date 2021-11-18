using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PowerBiAuditApp.Client.Services
{
    public interface IGraphService
    {
        Task<Dictionary<string, Guid>> GetGroupIds(params string[] groupNames);
    }
}