using AnBlog.Api.Extensions;
using AnBlog.Api.Filters;
using AnBlog.Core.Domain.Identity;
using AnBlog.Core.Models;
using AnBlog.Core.Models.System.Users;
using AnBlog.Core.SeedWorks.Constants;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AnBlog.Api.Controllers.AdminApi
{
    public class UsersController : AdminControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;

        public UsersController(UserManager<AppUser> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        [HttpGet("{id}")]
        [Authorize(Permissions.Users.View)]
        public async Task<ActionResult<UserDto>> GetUserById(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Id is null.");
            }

            var user = await _userManager.FindByIdAsync(id);

            if (user is null)
            {
                return NotFound("No user was found.");
            }

            var mappedUser = _mapper.Map<UserDto>(user);
            var roles = await _userManager.GetRolesAsync(user);
            mappedUser.Roles = roles;

            return Ok(mappedUser);
        }

        [HttpGet("paging")]
        [Authorize(Permissions.Users.View)]
        public async Task<ActionResult<PagedResult<UserDto>>> GetAllUserPaging(string? keyword, 
                                                                                int pageIndex = 1, 
                                                                                int pageSize = 10)
        {
            var query = _userManager.Users;

            if (string.IsNullOrWhiteSpace(keyword) == false)
            {
                query = query.Where(u => u.FirstName.Contains(keyword) || 
                                         u.LastName.Contains(keyword) ||
                                         u.Email.Contains(keyword) ||
                                         u.PhoneNumber.Contains(keyword));
            }

            var totalRow = await query.CountAsync();

            var skipRow = (pageIndex - 1 < 0 ? 1 : pageIndex - 1) * pageIndex; 
            query = query.Skip(skipRow).Take(pageSize);

            var pagedResult = new PagedResult<UserDto>
            {
                CurrentPage = pageIndex,
                PageSize = pageSize,
                RowCount = totalRow,
                Results = await _mapper.ProjectTo<UserDto>(query).ToListAsync()
        };

            return pagedResult;
        }

        [HttpPut("{id}")]
        [ValidateModel]
        [Authorize(Permissions.Users.Edit)]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest updateUserRequest)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());

            if (user is null)
            {
                return NotFound("No user was found");
            }

            _mapper.Map(updateUserRequest, user);
            var result = await _userManager.UpdateAsync(user);

            return result.Succeeded ? Ok() : BadRequest(string.Join("<br>", result.Errors.Select(x => x.Description)));
        }

        [HttpPost]
        [ValidateModel]
        [Authorize(Permissions.Users.Create)]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest createUserRequest)
        {
            var userWithEmailFound= await _userManager.FindByEmailAsync(createUserRequest.Email);            

            if (userWithEmailFound is not null)
            {
                return BadRequest("Email is duplicated. Please choose the difference email.");
            }

            var userWithUsernameFound = await _userManager.FindByNameAsync(createUserRequest.UserName);

            if (userWithUsernameFound is not null)
            {
                return BadRequest("Username is duplicated. Please choose the difference username");
            }

            var newUser = _mapper.Map<AppUser>(createUserRequest);
            var result = await _userManager.CreateAsync(newUser);


            return result.Succeeded ? Ok() : BadRequest(string.Join("<br>", result.Errors.Select(x => x.Description)));
        }

        [HttpPut("password-change-current-user")]
        [ValidateModel]
        [Authorize(Permissions.Users.Edit)]
        public async Task<IActionResult> ChangeMyPassword([FromBody] ChangeMyPasswordRequest changeMyPasswordRequest)
        {
            var currentUser = await _userManager.FindByIdAsync(User.GetUserId().ToString());

            if (currentUser is null) 
            {
                return NotFound("No user was found.");
            }

            var result  = await _userManager.ChangePasswordAsync(currentUser, changeMyPasswordRequest.OldPassword, changeMyPasswordRequest.NewPassword);

            return result.Succeeded ? Ok() : BadRequest(string.Join("<br>", result.Errors.Select(x => x.Description)));
        }

        [HttpDelete]
        [Authorize(Permissions.Users.Delete)]
        public async Task<IActionResult> DeleteUsers([FromQuery] string[] ids)
        {
            foreach (var id in ids)
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }
                await _userManager.DeleteAsync(user);
            }
            return Ok();
        }

        [HttpPost("set-password/{id}")]
        [Authorize(Permissions.Users.Edit)]
        public async Task<IActionResult> SetPassword(Guid id, [FromBody] SetPasswordRequest model)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());

            if (user is null)
            {
                return NotFound("No user was found");
            }

            user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, model.NewPassword);
            var result = await _userManager.UpdateAsync(user);

            return result.Succeeded ? Ok() : BadRequest(string.Join("<br>", result.Errors.Select(x => x.Description)));
        }

        [HttpPost("change-email/{id}")]
        [Authorize(Permissions.Users.Edit)]
        public async Task<IActionResult> ChangeEmail(Guid id, [FromBody] ChangeEmailRequest request)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());

            if (user is null)
            {
                return NotFound("No user was found");
            }

            var token = await _userManager.GenerateChangeEmailTokenAsync(user, request.Email);
            var result = await _userManager.ChangeEmailAsync(user, request.Email, token);

            return result.Succeeded ? Ok() : BadRequest(string.Join("<br>", result.Errors.Select(x => x.Description)));
        }

        [HttpPut("{id}/assign-users")]
        [Authorize(Permissions.Users.Edit)]
        public async Task<IActionResult> AssignRolesToUser(string id, [FromBody] string[] roles)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());

            if (user is null)
            {
                return NotFound("No user was found");
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            var resultCurrentRolesRemoved = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            var resultNewRolesAdded = await _userManager.AddToRolesAsync(user, roles);

            if (!resultCurrentRolesRemoved.Succeeded || !resultNewRolesAdded.Succeeded)
            {
                var removedErrorList = resultCurrentRolesRemoved.Errors;
                var addedErrorList = resultNewRolesAdded.Errors;
                var errorList = removedErrorList.Union(addedErrorList);

                return BadRequest(string.Join("<br/>", errorList.Select(x => x.Description)));
            }

            return Ok();
        }

    }
}
