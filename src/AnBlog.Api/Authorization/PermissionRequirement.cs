using Microsoft.AspNetCore.Authorization;

namespace AnBlog.Api.Authorization
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string Permission { get; private set; }
        public PermissionRequirement(string permission) 
            => Permission = permission;
    }
}
