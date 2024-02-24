using AnBlog.Api.Extensions;
using AnBlog.Api.Services;
using AnBlog.Core.Domain.Identity;
using AnBlog.Core.Models.Auth;
using AnBlog.Core.Models.System;
using AnBlog.Core.SeedWorks.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;

namespace AnBlog.Api.Controllers.AdminApi
{    
    public class AuthController : AdminControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenService _tokenService;

        public AuthController(UserManager<AppUser> userManager,
                              SignInManager<AppUser> signInManager,
                              ITokenService tokenService,
                              RoleManager<AppRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _roleManager = roleManager;
        }

        [HttpPost]
        public async Task<ActionResult<AuthenticatedResult>> Login([FromBody] LoginRequest loginRequest)
        {
            if (loginRequest == null)
            {
                return BadRequest();
            }

            var user = await _userManager.FindByNameAsync(loginRequest.Username);

            if (user == null || user.IsActive == false || user.LockoutEnabled)
            {
                return Unauthorized();
            }

            // Authen
            var result = await _signInManager.PasswordSignInAsync(user, loginRequest.Password, false, true);

            if (!result.Succeeded)
            {
                return Unauthorized();
            }

            // Author
            var roles = await _userManager.GetRolesAsync(user);

            var permissions = this.GetPermissionsByUserIdAsync(user.Id.ToString());

            var claims = new[]
            {
                  new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                  new Claim(ClaimTypes.NameIdentifier, user.UserName ?? string.Empty),
                  new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                  new Claim(UserClaims.Id, user.Id.ToString()),
                  new Claim(UserClaims.FirstName, user.FirstName),
                  new Claim(UserClaims.Roles, string.Join(";", roles)),
                  new Claim(UserClaims.Permissions, JsonSerializer.Serialize(permissions)),
                  new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var accessToken = _tokenService.GenerateAccessToken(claims);
            var refreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(30);

            await _userManager.UpdateAsync(user);

            return Ok(new AuthenticatedResult
            {
                RefreshToken = refreshToken,
                Token = accessToken,
            });
        }

        private async Task<List<string>> GetPermissionsByUserIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);            
            var roles = await _userManager.GetRolesAsync(user!);

            var permissions = new List<string>();


            if (roles.Contains(Roles.Admin))
            {
                var allPermissions = new List<RoleClaimsDto>();
                
                var types = typeof(Permissions).GetTypeInfo().DeclaredNestedTypes;

                foreach (var type in types)
                {
                    allPermissions.GetPermissions(type);
                }
                
                if (allPermissions.Any())
                {
                    permissions.AddRange(allPermissions.Select(p => p.Value)!);
                }
            }
            else
            {
                foreach (var roleName in roles)
                {
                    var role = await _roleManager.FindByNameAsync(roleName);
                    var claims = await _roleManager.GetClaimsAsync(role!);
                    var roleClaimValues = claims.Select(c => c.Value).ToList();
                    permissions.AddRange(roleClaimValues);
                }
            }

            return permissions.Distinct().ToList();
        }

    }
}
