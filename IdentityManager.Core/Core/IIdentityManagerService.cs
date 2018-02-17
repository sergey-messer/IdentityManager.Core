using System.Collections.Generic;
using System.Threading.Tasks;
using TzIdentityManager.Core.Metadata;

namespace TzIdentityManager.Core
{
    public interface IIdentityManagerService
    {
        Task<IdentityManagerMetadata> GetMetadataAsync();

        // users
        Task<IdentityManagerResult<CreateResult>> CreateUserAsync(IEnumerable<PropertyValue> properties);
        Task<IdentityManagerResult> DeleteUserAsync(string subject);

        Task<IdentityManagerResult<QueryResult<UserSummary>>> QueryUsersAsync(string filter, int start, int count);
        Task<IdentityManagerResult<UserDetail>> GetUserAsync(string subject);

        Task<IdentityManagerResult> SetUserPropertyAsync(string subject, string type, string value);

        Task<IdentityManagerResult> AddUserClaimAsync(string subject, string type, string value);
        Task<IdentityManagerResult> RemoveUserClaimAsync(string subject, string type, string value);

        // roles
        Task<IdentityManagerResult> AddUserRoleAsync(string subject, string value);
        Task<IdentityManagerResult> RemoveUserRoleAsync(string subject, string value);


        Task<IdentityManagerResult<CreateResult>> CreateRoleAsync(IEnumerable<PropertyValue> properties);
        Task<IdentityManagerResult> DeleteRoleAsync(string subject);

        Task<IdentityManagerResult<QueryResult<RoleSummary>>> QueryRolesAsync(string filter, int start, int count);
        Task<IdentityManagerResult<RoleDetail>> GetRoleAsync(string subject);

        Task<IdentityManagerResult> SetRolePropertyAsync(string subject, string type, string value);
    }
}
