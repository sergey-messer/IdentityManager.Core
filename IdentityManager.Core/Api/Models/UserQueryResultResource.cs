﻿using System;
using System.Collections.Generic;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using TzIdentityManager.Core;
using TzIdentityManager.Core.Metadata;

namespace TzIdentityManager.Api.Models
{
    public class UserQueryResultResource
    {
        public UserQueryResultResource(QueryResult<UserSummary> result, IUrlHelper url, UserMetadata meta)
        {
            if (result == null) throw new ArgumentNullException("result");
            if (url == null) throw new ArgumentNullException("url");
            if (meta == null) throw new ArgumentNullException("meta");

            Data = new UserQueryResultResourceData(result, url, meta);

            var links = new Dictionary<string, object>();
            if (meta.SupportsCreate)
            {
                links["create"] = new CreateUserLink(url, meta);
            };
            Links = links;
        }

        public UserQueryResultResourceData Data { get; set; }
        public object Links { get; set; }
    }

    public class UserQueryResultResourceData : QueryResult<UserSummary>
    {
        //static UserQueryResultResourceData()
        //{
        //    AutoMapper.Mapper.CreateMap<QueryResult<UserSummary>, UserQueryResultResourceData>()
        //        .ForMember(x => x.Items, opts => opts.MapFrom(x => x.Items));
        //    AutoMapper.Mapper.CreateMap<UserSummary, UserResultResource>()
        //        .ForMember(x => x.Data, opts => opts.MapFrom(x => x));
        //}

        public UserQueryResultResourceData(QueryResult<UserSummary> result, IUrlHelper url, UserMetadata meta)
        {
            if (result == null) throw new ArgumentNullException("result");
            if (url == null) throw new ArgumentNullException("url");
            if (meta == null) throw new ArgumentNullException("meta");
            
            Mapper.Map(result, this);

            foreach (var user in this.Items)
            {
                var links = new Dictionary<string, string> {
                    {"detail", url.Link(Constants.RouteNames.GetUser, new { subject = user.Data.Subject })}
                };
                if (meta.SupportsDelete)
                {
                    links.Add("delete", url.Link(Constants.RouteNames.DeleteUser, new { subject = user.Data.Subject }));
                }
                user.Links = links;
            }
        }

        public new IEnumerable<UserResultResource> Items { get; set; }
    }

    public class UserResultResource
    {
        public UserSummary Data { get; set; }
        public object Links { get; set; }
    }
}
