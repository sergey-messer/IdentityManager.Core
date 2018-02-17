using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TzIdentityManager.Api.Models;
using TzIdentityManager.Core;
using TzIdentityManager.Core.Metadata;

namespace TzIdentityManager.Api.Controllers
{
    [Route(Constants.MetadataRoutePrefix)]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public class MetaController : Controller
    {
        IIdentityManagerService userManager;
        //IdentityManagerOptions config;
        public MetaController(/*IdentityManagerOptions config,*/ IIdentityManagerService userManager)
        {
            //if (config == null) throw new ArgumentNullException("config");
            if (userManager == null) throw new ArgumentNullException("userManager");

            //this.config = config;
            this.userManager = userManager;
        }

        IdentityManagerMetadata _metadata;
        async Task<IdentityManagerMetadata> GetMetadataAsync()
        {
            if (_metadata == null)
            {
                _metadata = await this.userManager.GetMetadataAsync();
                if (_metadata == null) throw new InvalidOperationException("GetMetadataAsync returned null");
                _metadata.Validate();
            }

            return _metadata;
        }

        [Route("")]
        public async Task<IActionResult> Get()
        {
            var meta = await GetMetadataAsync();

            var data = new Dictionary<string, object>();
            
            var cp = (ClaimsPrincipal)User;
            var name = cp.Identity.Name;
            data.Add("currentUser", new {
                username = name
            });

            var links = new Dictionary<string, object>();
            links["users"] = Url.Link(Constants.RouteNames.GetUsers, null);
            if (meta.RoleMetadata.SupportsListing)
            {
                links["roles"] = Url.Link(Constants.RouteNames.GetRoles, null);
            }
            if (meta.UserMetadata.SupportsCreate)
            {
                links["createUser"] = new CreateUserLink(Url, meta.UserMetadata);
            }
            if (meta.RoleMetadata.SupportsCreate)
            {
                links["createRole"] = new CreateRoleLink(Url, meta.RoleMetadata);
            }

            return Ok(new 
            {
                Data = data,
                Links = links
            });
        }
    }
}
