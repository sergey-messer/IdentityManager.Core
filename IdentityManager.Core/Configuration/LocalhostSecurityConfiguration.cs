using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Builder;

namespace TzIdentityManager.Configuration
{
    public class LocalhostSecurityConfiguration : HostSecurityConfiguration
    {
        public LocalhostSecurityConfiguration()
        {
            HostAuthenticationType = Constants.LocalAuthenticationType;
        }

        public override void Configure(IApplicationBuilder app)
        {
            if (this.ShowLoginButton == null)
            {
                this.ShowLoginButton = false;
            }
            //app.Use<LocalhostAuthenticationMiddleware>(new LocalhostAuthenticationOptions(this));

            base.Configure(app);
        }
    }
}
