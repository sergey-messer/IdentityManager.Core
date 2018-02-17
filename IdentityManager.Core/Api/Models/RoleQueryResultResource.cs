using System;
using System.Collections.Generic;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using TzIdentityManager.Core;
using TzIdentityManager.Core.Metadata;

namespace TzIdentityManager.Api.Models
{
    public class RoleQueryResultResource
    {
        public RoleQueryResultResource(QueryResult<RoleSummary> result, IUrlHelper url, RoleMetadata meta)
        {
            if (result == null) throw new ArgumentNullException("result");
            if (url == null) throw new ArgumentNullException("url");
            if (meta == null) throw new ArgumentNullException("meta");

            Data = new RoleQueryResultResourceData(result, url, meta);

            var links = new Dictionary<string, object>();
            if (meta.SupportsCreate)
            {
                links["create"] = new CreateRoleLink(url, meta);
            };
            Links = links;
        }

        public RoleQueryResultResourceData Data { get; set; }
        public object Links { get; set; }
    }

    public class RoleQueryResultResourceData : QueryResult<RoleSummary>
    {
        
        public RoleQueryResultResourceData(QueryResult<RoleSummary> result, IUrlHelper url, RoleMetadata meta)
        {
            if (result == null) throw new ArgumentNullException("result");
            if (url == null) throw new ArgumentNullException("url");
            if (meta == null) throw new ArgumentNullException("meta");

            Mapper.Map(result, this);

            foreach (var role in this.Items)
            {
                var links = new Dictionary<string, string>();
                links.Add("detail", url.Link(Constants.RouteNames.GetRole, new { subject = role.Data.Subject }));
                if (meta.SupportsDelete)
                {
                    links.Add("delete", url.Link(Constants.RouteNames.DeleteRole, new { subject = role.Data.Subject }));
                }
                role.Links = links;
            }
        }

        public new IEnumerable<RoleResultResource> Items { get; set; }
    }

    public class RoleResultResource
    {
        public RoleSummary Data { get; set; }
        public object Links { get; set; }
    }
}
