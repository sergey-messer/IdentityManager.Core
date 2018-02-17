using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using TzIdentityManager.Core;
using TzIdentityManager.Core.Extensions;
using TzIdentityManager.Core.Metadata;

namespace TzIdentityManager.Api.Models
{
    public class RoleDetailResource
    {
        public RoleDetailResource(RoleDetail role, IUrlHelper url, RoleMetadata meta)
        {
            if (role == null) throw new ArgumentNullException("role");
            if (url == null) throw new ArgumentNullException("url");
            if (meta == null) throw new ArgumentNullException("meta");

            Data = new RoleDetailDataResource(role, url, meta);

            var links = new Dictionary<string, string>();
            if (meta.SupportsDelete)
            {
                links["delete"] = url.Link(Constants.RouteNames.DeleteRole, new { subject = role.Subject });
            }
            this.Links = links;
        }

        public RoleDetailDataResource Data { get; set; }
        public object Links { get; set; }
    }
    
    public class RoleDetailDataResource : Dictionary<string, object>
    {
        public RoleDetailDataResource(RoleDetail role, IUrlHelper url, RoleMetadata meta)
        {
            if (role == null) throw new ArgumentNullException("role");
            if (url == null) throw new ArgumentNullException("url");
            if (meta == null) throw new ArgumentNullException("meta");

            this["name"] = role.Name;
            this["subject"] = role.Subject;

            if (role.Properties != null)
            {
                var props =
                    from p in role.Properties
                    let m = (from m in meta.UpdateProperties where m.Type == p.Type select m).SingleOrDefault()
                    where m != null
                    select new
                    {
                        Data = m.Convert(p.Value),
                        Meta = m,
                        Links = new
                        {
                            update = url.Link(Constants.RouteNames.UpdateRoleProperty,
                                new
                                {
                                    subject = role.Subject,
                                    type = p.Type.ToBase64UrlEncoded()
                                })
                        }
                    };

                if (props.Any())
                {
                    this["properties"] = props.ToArray();
                }
            }
        }
    }
}
