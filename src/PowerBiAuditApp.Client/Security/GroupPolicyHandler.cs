using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace PowerBiAuditApp.Client.Security
{
    public class GroupPolicyHandler : AuthorizationHandler<GroupRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, GroupRequirement requirement)
        {
            var userGroups = context.User.Claims
                .Where(x => x.Type == "groups")
                .Select(x => new Guid(x.Value))
                .ToArray();

            if (userGroups.Contains(requirement.GroupId))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}