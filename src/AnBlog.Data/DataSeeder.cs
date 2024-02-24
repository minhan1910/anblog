﻿using AnBlog.Core.Domain.Identity;
using AnBlog.Core.SeedWorks.Constants;
using Microsoft.AspNetCore.Identity;

namespace AnBlog.Data
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(AnBlogContext context, 
                                           RoleManager<AppRole> roleManager)
        {
            var passwordHasher = new PasswordHasher<AppUser>();

            var rootAdminRoleId = Guid.NewGuid();

            var role = new AppRole 
            {
                Id = rootAdminRoleId,
                Name = Roles.Admin,
                NormalizedName = Roles.Admin.ToUpperInvariant(),
                DisplayName = "Quản trị viên",
            };

            if (!context.Roles.Any())
            {
                await context.Roles.AddAsync(role);
                await context.SaveChangesAsync();
            }

            if (!context.Users.Any())
            {
                var userId = Guid.NewGuid();
                var userEmail = "admin@gmail.com";
                var userName = "admin";
                var user = new AppUser
                {
                    Id = userId,
                    FirstName = "An",
                    LastName = "Minh",
                    Email = userEmail,
                    NormalizedEmail = userEmail.ToUpperInvariant(),
                    UserName = userName,
                    NormalizedUserName = userName.ToUpperInvariant(),
                    IsActive = true,
                    // ko co cai nay ko login duoc
                    SecurityStamp = Guid.NewGuid().ToString(),
                    LockoutEnabled = false,
                    DateCreated = DateTime.Now,
                };
                user.PasswordHash = passwordHasher.HashPassword(user, "Admin123@");

                await context.Users.AddAsync(user);

                await context.UserRoles.AddAsync(new IdentityUserRole<Guid>
                {
                    RoleId = rootAdminRoleId,
                    UserId = userId
                });

                await context.SaveChangesAsync();
            }

            // seed permissions
            var permissions = await roleManager.GetClaimsAsync(role);
            if (permissions.Any() == false)
            {

            }
        }

    }
}