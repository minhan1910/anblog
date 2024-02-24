using AnBlog.Api.Extensions;
using AnBlog.Api.Filters;
using AnBlog.Core.Domain.Identity;
using AnBlog.Core.Models;
using AnBlog.Core.Models.System;
using AnBlog.Core.SeedWorks.Constants;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace AnBlog.Api.Controllers.AdminApi
{
    public class RoleController : AdminControllerBase
    {
        private readonly RoleManager<AppRole> _roleManager;
        private readonly IMapper _mapper;

        public RoleController(RoleManager<AppRole> roleManager, IMapper mapper)
        {
            _roleManager = roleManager;
            _mapper = mapper;
        }

        [HttpPost]
        [ValidateModel]
        [Authorize(Permissions.Roles.Create)]
        public async Task<IActionResult> CreateRole([FromBody] CreateUpdateRoleRequest createUpdateRoleRequest)
        {
            var roleExists = await _roleManager.FindByNameAsync(createUpdateRoleRequest.Name);

            if (roleExists is not null)
            {
                return BadRequest("Role name is duplicated. Please create other role name.");
            }

            var result = await _roleManager.CreateAsync(new AppRole
            {
                DisplayName = createUpdateRoleRequest.DisplayName,
                Name = createUpdateRoleRequest.Name,
            });

            return result.Succeeded ? Ok(result) : BadRequest();
        }

        [HttpPut("{id}")]
        [ValidateModel]
        [Authorize(Permissions.Roles.Edit)]
        public async Task<IActionResult> UpdateRole(string id, [FromBody] CreateUpdateRoleRequest createUpdateRoleRequest)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Id ");
            }

            var roleInDb = await _roleManager.FindByIdAsync(id);

            if (roleInDb is null)
            {
                return NotFound("No role was found");
            }

            roleInDb.DisplayName = createUpdateRoleRequest.DisplayName;
            roleInDb.Name = createUpdateRoleRequest.Name;
            var result = await _roleManager.UpdateAsync(roleInDb);

            return result.Succeeded ? Ok(result) : BadRequest();
        }

        [HttpDelete]
        [Authorize(Permissions.Roles.Delete)]
        public async Task<IActionResult> DeleteRoles([FromQuery] Guid[] roleIds)
        {
            foreach (var roleId in roleIds)
            {

                var roleInDb = await _roleManager.FindByIdAsync(roleId.ToString());

                if (roleInDb is null)
                {
                    return NotFound("No role was found");
                }

                var resultAfterDeleting = await _roleManager.DeleteAsync(roleInDb);

                if (resultAfterDeleting.Succeeded == false)
                {
                    return BadRequest("Delete Role failed");
                }
            }

            return Ok("Delete Role(s) Successfully!");
        }

        [HttpGet("{id}")]
        [Authorize(Permissions.Roles.View)]
        public async Task<ActionResult<RoleDto>> GetRoleById(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Id is null or empty");
            }

            var roleInDb = await _roleManager.FindByIdAsync(id);

            if (roleInDb is null)
            {
                return NotFound("No role was found.");
            }

            var mappedRole = _mapper.Map<RoleDto>(roleInDb);

            return Ok(mappedRole);
        }

        [HttpGet]
        [Authorize(Permissions.Roles.View)]
        public async Task<ActionResult<List<RoleDto>>> GetAllRoles()
        {
            var model = await _mapper.ProjectTo<RoleDto>(_roleManager.Roles).ToListAsync();

            return Ok(model);
        }

        [HttpGet]
        [Route("paging")]
        [Authorize(Permissions.Roles.View)]
        public async Task<ActionResult<PagedResult<RoleDto>>> GetAllRolesPaging(string? keyword, 
                                                                                int pageIndex = 1,
                                                                                int pageSize = 10)
        {
            var query = _roleManager.Roles;

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(role => role.Name!.Contains(keyword) || role.DisplayName.Contains(keyword));
            }

            var totalRow = await query.CountAsync();
            
            var skipRow = (pageIndex - 1 < 0 ? 1 : pageIndex - 1) * pageIndex;

            query =
                query
                .Skip(skipRow)
                .Take(pageSize);

            var data = await _mapper.ProjectTo<RoleDto>(query).ToListAsync();

            var response = new PagedResult<RoleDto>
            {
                CurrentPage = pageIndex,
                PageSize = pageSize,
                RowCount = totalRow,
                Results = data                
            };

            return Ok(response);
        }

        [HttpGet("{id}/permisisons")]
        [Authorize(Permissions.Roles.View)]
        public async Task<ActionResult<PermissionDto>> GetAllRolePermissions(string id)
        {            
            var allPermissions = new List<RoleClaimsDto>();

            var permissionsNestedType = typeof(Permissions).GetTypeInfo().DeclaredNestedTypes;

            foreach (var permissionType in permissionsNestedType)
            {
                allPermissions.GetPermissions(permissionType);
            }

            var role = await _roleManager.FindByIdAsync(id);

            if (role is null)
            {
                return NotFound("No role was found.");
            }

            var curentRoleClaims = await _roleManager.GetClaimsAsync(role);

            var currentRoleClaimValues = curentRoleClaims.Select(crl => crl.Value);
            var allPermissionValues = allPermissions.Select(x => x.Value);

            var authorizedClaims = currentRoleClaimValues.Intersect(allPermissionValues).ToHashSet();

            foreach (var permission in allPermissions)
            {
                if (authorizedClaims.Contains(permission.Value))
                {
                    permission.Selected = true;
                }
            }

            return Ok(new PermissionDto
            {
                RoleId = role.Id.ToString(),
                RoleClaims = allPermissions,
            });
        }

        [HttpPut]
        [Authorize(Permissions.Roles.Edit)]
        public async Task<IActionResult> SavePermissions([FromBody] PermissionDto permission)
        {
            var role = await _roleManager.FindByIdAsync(permission.RoleId);

            if (role is null)
            {
                return NotFound("No role was found.");
            }

            var userPermissionsModified =
                permission
                .RoleClaims
                .Where(rl => rl.Selected)
                .Select(rl => rl.Value)
                .ToList();

            var currentlyUserPermissions = await _roleManager.GetClaimsAsync(role);

            foreach (var userPermission in currentlyUserPermissions)
            {
                await _roleManager.RemoveClaimAsync(role, userPermission);
            }

            foreach (var userPermissionModified in userPermissionsModified)
            {
                await _roleManager.AddPermissionClaim(role, userPermissionModified!);
            }

            return Ok();
        }

    }
}