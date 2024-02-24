using AnBlog.Core.Domain.Identity;
using AnBlog.Core.Models.System;
using AnBlog.Core.SeedWorks.Constants;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel;
using System.Reflection;
using System.Security.Claims;

namespace AnBlog.Api.Extensions
{
    public static class ClaimExtensions
    {
        public static void GetPermissions(this List<RoleClaimsDto> allPermissions, Type policy)
        {
            FieldInfo[] fields = policy.GetFields(BindingFlags.Static | BindingFlags.Public);

            foreach (FieldInfo field in fields)
            {                
                string displayName = field.GetValue(null)!.ToString()!;
                var attributes = field.GetCustomAttributes(typeof(DescriptionAttribute), true);

                if (attributes.Length > 0)
                {
                    var description = (DescriptionAttribute)attributes[0];
                    displayName = description.Description;
                }

                allPermissions.Add(new RoleClaimsDto
                {
                    Value = field.GetValue(null)!.ToString()!,
                    Type = UserClaims.Permissions,
                    DisplayName = displayName,                    
                });
            }
        }

        public static async Task AddPermissionClaim(this RoleManager<AppRole> roleManager, AppRole appRole, string permission)
        {
            var allClaims = await roleManager.GetClaimsAsync(appRole);

            if (!allClaims.Any(p => p.ValueType == UserClaims.Permissions && p.Value == permission))
            {
                await roleManager.AddClaimAsync(appRole, new Claim(UserClaims.Permissions, permission));
            }
        }
    }
}
