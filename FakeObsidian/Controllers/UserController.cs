using AutoMapper;
using FakeObsidian.Api.Models.User;
using FakeObsidian.Application.DTO;
using FakeObsidian.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FakeObsidian.Api.Controllers
{
    [Route("api/users")]
    [ApiController]
    [Authorize]
    public class UserController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager,
        IMapper mapper) : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        private readonly IMapper _mapper = mapper;

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUsers()
        {
            var users = _userManager.Users.ToList();
            var result = new List<UserDto>();
            foreach(var user in users)
                {
                var roles = await _userManager.GetRolesAsync(user);
                result.Add(new UserDto
                {
                    Id = user.Id,
                    Avatar = user.Avatar,
                    UserName = user.UserName,
                    Email = user.Email,
                    Roles = roles
                });
            }
            if (User.IsInRole("Admin"))
            {
                var response = _mapper.Map<List<UserResponse>>(result);
                return Ok(new { response });
            }
            else if (User.IsInRole("User"))
            {
                var response = _mapper.Map<List<LimitedUserResponse>>(result);
                return Ok(new { response });
            }
            else return Forbid();
        }

        [HttpGet("{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            if (User.IsInRole("Admin"))
            {
                var response = _mapper.Map<UserResponse>(user);
                response.Roles = (List<string>)roles;
                return Ok(new { response });
            }
            else if (User.IsInRole("User"))
            {
                var response = _mapper.Map<LimitedUserResponse>(user);
                response.Roles = (List<string>)roles;
                return Ok(new { response });
            }
            else return Forbid();
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateUser(string userId, [FromBody] UpdateUserRequest request)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);
            var rolesToAdd = request.Roles.Except(currentRoles);
            var rolesToRemove = currentRoles.Except(request.Roles);

            user.Avatar = request.Avatar;
            user.UserName = request.UserName;
            user.Email = request.Email;
            await _userManager.UpdateAsync(user);

            await _userManager.AddToRolesAsync(user, rolesToAdd);
            await _userManager.RemoveFromRolesAsync(user, rolesToRemove);

            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            await _userManager.DeleteAsync(user);

            return NoContent();
        }
    }
}
