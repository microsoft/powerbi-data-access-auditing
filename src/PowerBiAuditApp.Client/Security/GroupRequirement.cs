using System;
using Microsoft.AspNetCore.Authorization;

namespace PowerBiAuditApp.Client.Security
{
    public class GroupRequirement : IAuthorizationRequirement
    {
        public Guid GroupId { get; }

        public GroupRequirement(Guid groupId)
        {
            GroupId = groupId;
        }
    }
}