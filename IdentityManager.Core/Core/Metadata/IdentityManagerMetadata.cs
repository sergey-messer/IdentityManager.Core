using System;

namespace TzIdentityManager.Core.Metadata
{
    public class IdentityManagerMetadata
    {
        public IdentityManagerMetadata()
        {
            this.UserMetadata = new UserMetadata();
            this.RoleMetadata = new RoleMetadata();
        }

        public UserMetadata UserMetadata { get; set; }
        public RoleMetadata RoleMetadata { get; set; }

        public void Validate()
        {
            if (UserMetadata == null) throw new InvalidOperationException("UserMetadata not assigned.");
            UserMetadata.Validate();

            if (RoleMetadata == null) throw new InvalidOperationException("RoleMetadata not assigned.");
            RoleMetadata.Validate();
        }
    }
}
