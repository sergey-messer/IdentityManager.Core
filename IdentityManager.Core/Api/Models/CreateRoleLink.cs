using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using TzIdentityManager.Core.Extensions;
using TzIdentityManager.Core.Metadata;

namespace TzIdentityManager.Api.Models
{
    public class CreateRoleLink : Dictionary<string, object>
    {
        public CreateRoleLink(IUrlHelper url, RoleMetadata roleMetadata)
        {
            this["href"] = url.Link(Constants.RouteNames.CreateRole, null);
            this["meta"] = roleMetadata.GetCreateProperties();
        }
    }
}
