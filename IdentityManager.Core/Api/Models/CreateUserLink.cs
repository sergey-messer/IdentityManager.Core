using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using TzIdentityManager.Core.Extensions;
using TzIdentityManager.Core.Metadata;

namespace TzIdentityManager.Api.Models
{
    public class CreateUserLink : Dictionary<string, object>
    {
        public CreateUserLink(IUrlHelper url, UserMetadata userMetadata)
        {
            this["href"] = url.Link(Constants.RouteNames.CreateUser, null);
            this["meta"] = userMetadata.GetCreateProperties();
        }
    }
}
