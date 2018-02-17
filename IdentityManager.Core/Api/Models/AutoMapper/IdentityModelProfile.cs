using AutoMapper;
using TzIdentityManager.Core;

namespace TzIdentityManager.Api.Models.AutoMapper
{
    public class IdentityModelProfile:Profile
    {

        public IdentityModelProfile()
        {
            CreateMap<QueryResult<UserSummary>, UserQueryResultResourceData>()
                .ForMember(x => x.Items, opts => opts.MapFrom(x => x.Items));
            CreateMap<UserSummary, UserResultResource>()
                .ForMember(x => x.Data, opts => opts.MapFrom(x => x));

            CreateMap<QueryResult<RoleSummary>, RoleQueryResultResourceData>()
                .ForMember(x => x.Items, opts => opts.MapFrom(x => x.Items));
            CreateMap<RoleSummary, RoleResultResource>()
                .ForMember(x => x.Data, opts => opts.MapFrom(x => x));

        }
    }
    
}
