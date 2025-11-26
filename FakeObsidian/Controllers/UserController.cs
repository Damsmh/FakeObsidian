using AutoMapper;
using FakeObsidian.Api.Models.User;
using FakeObsidian.Application.DTO;
using FakeObsidian.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace FakeObsidian.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class UserController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager,
        IMapper mapper) : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        private readonly IMapper _mapper = mapper;

        [HttpGet("getAll")]
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
        public async Task<ActionResult<List<UserResponse>>> GetUser(string userId)
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
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateUserbyId(string userId, [FromBody] UpdateUserRequest request)
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

        [HttpPut("updateInfo")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateUserInfo([FromBody] UpdateUserRequest request)
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                         ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null) return NotFound();
                user.Avatar = request.Avatar;
                user.UserName = request.UserName;
                user.Email = request.Email;
                await _userManager.UpdateAsync(user);
            }

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

        //TODO: Add Notification system

        //[HttpPut("addFriend")]
        //[ProducesResponseType(StatusCodes.Status204NoContent)]
        //[ProducesResponseType(StatusCodes.Status401Unauthorized)]
        //[ProducesResponseType(StatusCodes.Status403Forbidden)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        //public async Task<IActionResult> SendFriendInvite([FromBody] AddFriendRequest request)
        //{
        //    var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
        //                 ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        //    if (!string.IsNullOrEmpty(userId))
        //    {
        //        var user = await _userManager.FindByIdAsync(userId);
        //        var newFriend = await _userManager.FindByIdAsync(request.Id);
        //        if (user == null || newFriend == null) return NotFound();
        //        newFriend.Notifications.Add(newFriend);
        //        await _userManager.UpdateAsync(user);
        //    }

        //    return NoContent();
        //}
    }
}
