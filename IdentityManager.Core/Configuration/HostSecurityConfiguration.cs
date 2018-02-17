using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace TzIdentityManager.Configuration
{
    public class HostSecurityConfiguration : SecurityConfiguration
    {
        public string HostAuthenticationType { get; set; }
        public string AdditionalSignOutType { get; set; }
        public TimeSpan TokenExpiration { get; set; }

        public HostSecurityConfiguration()
        {
            TokenExpiration = Constants.DefaultTokenExpiration;
        }

        internal override void Validate()
        {
            base.Validate();

            if (String.IsNullOrWhiteSpace(HostAuthenticationType)) throw new Exception("HostAuthenticationType is required.");
        }

        public override void Configure(IApplicationBuilder app)
        {
            //app.UseOAuthAuthorizationServer(this);
        }

        internal override void SignOut(HttpContext context)
        {
            context.SignOutAsync(this.HostAuthenticationType);
            if (!String.IsNullOrWhiteSpace(AdditionalSignOutType))
            {
                context.SignOutAsync(AdditionalSignOutType);
            }
        }
    }
}
