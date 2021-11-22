using Microsoft.AspNetCore.Authorization;

namespace PowerBiAuditApp.Client.Security
{
    public class AdministratorAuthorizeAttribute : AuthorizeAttribute
    {
        public const string PolicyName = "Administrator";
        public AdministratorAuthorizeAttribute()
        {
            Policy = PolicyName;
        }
    }
}