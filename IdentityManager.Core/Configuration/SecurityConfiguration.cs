using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace TzIdentityManager.Configuration
{
    public abstract class SecurityConfiguration
    {
        public bool RequireSsl { get; set; }
        public string BearerAuthenticationType { get; set; }

        public string NameClaimType { get; set; }
        public string RoleClaimType { get; set; }

        public string AdminRoleName { get; set; }

        public virtual bool? ShowLoginButton { get; set; }

        internal SecurityConfiguration()
        {
            RequireSsl = true;
            BearerAuthenticationType = Constants.BearerAuthenticationType;

            NameClaimType = Constants.ClaimTypes.Name;
            RoleClaimType = Constants.ClaimTypes.Role;
            AdminRoleName = Constants.AdminRoleName;
        }

        internal virtual void Validate()
        {
            if (String.IsNullOrWhiteSpace(BearerAuthenticationType))
            {
                throw new Exception("BearerAuthenticationType is required.");
            }
            if (String.IsNullOrWhiteSpace(NameClaimType))
            {
                throw new Exception("NameClaimType is required.");
            }
            if (String.IsNullOrWhiteSpace(RoleClaimType))
            {
                throw new Exception("RoleClaimType is required.");
            }
            if (String.IsNullOrWhiteSpace(AdminRoleName))
            {
                throw new Exception("AdminRoleName is required.");
            }
        }

        public abstract void Configure(IApplicationBuilder app);

        internal virtual void SignOut(HttpContext context)
        {
        }
    }
}
