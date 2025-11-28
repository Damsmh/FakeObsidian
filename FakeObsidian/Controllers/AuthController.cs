using AutoMapper;
using FakeObsidian.Api.Models.Auth;
using FakeObsidian.Api.Models.User;
using FakeObsidian.Domain.Entities;
using FakeObsidian.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace FakeObsidian.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController(
        UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager,
        IConfiguration configuration, AppDbContext db, IMapper mapper) : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        private readonly IConfiguration _configuration = configuration;
        private readonly AppDbContext _db = db;
        private readonly IMapper _mapper = mapper;

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegRequest model)
        {
            if (await _userManager.FindByEmailAsync(model.Email) != null)
                return BadRequest(new { Error = "Пользователь с таким email уже существует" });

            var user = new AppUser { UserName = model.UserName, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            if (!await _roleManager.RoleExistsAsync("User"))
                await _roleManager.CreateAsync(new IdentityRole("User"));

            await _userManager.AddToRoleAsync(user, "User");

            var (accessToken, accessExp) = await CreateAccessTokenAsync(user);
            var refresh = CreateRefreshToken(user.Id);

            _db.RefreshTokens.Add(refresh);
            await _db.SaveChangesAsync();

            var moscowExpiration = ToMoscowTime(accessExp);

            var response = new AuthResponse
            {
                Expiration = moscowExpiration,
                Token = accessToken,
                RefreshToken = refresh.Token
            };

            return Ok(response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LogRequest model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                return Unauthorized(new { Error = "Неверный логин или пароль" });

            var (accessToken, accessExp) = await CreateAccessTokenAsync(user);
            var refresh = CreateRefreshToken(user.Id);

            _db.RefreshTokens.Add(refresh);
            await _db.SaveChangesAsync();

            var moscowExpiration = ToMoscowTime(accessExp);

            var response = new AuthResponse
            {
                Expiration = moscowExpiration,
                Token = accessToken,
                RefreshToken = refresh.Token
            };

            return Ok(response);
        }


        [HttpPost("refresh")]
        [Authorize]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
                return BadRequest();

            var existing = await _db.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken
                                          && rt.Revoked == null
                                          && rt.Expires > DateTime.UtcNow);

            if (existing == null)
                return Unauthorized();

            existing.Revoked = DateTime.UtcNow;

            var user = await _userManager.FindByIdAsync(existing.UserId);
            if (user == null)
                return Unauthorized();

            var (newAccessToken, newAccessExp) = await CreateAccessTokenAsync(user);
            var newRefresh = CreateRefreshToken(user.Id);

            _db.RefreshTokens.Add(newRefresh);
            await _db.SaveChangesAsync();

            var moscowExpiration = ToMoscowTime(newAccessExp);

            var response = new AuthResponse
            {
                Expiration = moscowExpiration,
                Token = newAccessToken,
                RefreshToken = newRefresh.Token
            };

            return Ok(response);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                var token = await _db.RefreshTokens
                    .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken
                                              && rt.Revoked == null
                                              && rt.Expires > DateTime.UtcNow);

                if (token != null)
                {
                    token.Revoked = DateTime.UtcNow;
                    await _db.SaveChangesAsync();
                }

                return Ok();
            }

            var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                      ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                var activeTokens = await _db.RefreshTokens
                    .Where(r => r.UserId == userId
                             && r.Revoked == null
                             && r.Expires > DateTime.UtcNow)
                    .ToListAsync();

                foreach (var t in activeTokens)
                {
                    t.Revoked = DateTime.UtcNow;
                }

                await _db.SaveChangesAsync();
            }

            return Ok();
        }

        private static RefreshToken CreateRefreshToken(string userId)
        {
            var randomBytes = RandomNumberGenerator.GetBytes(64);
            return new RefreshToken
            {
                Token = Convert.ToBase64String(randomBytes),
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow,
                UserId = userId
            };
        }

        private async Task<(string token, DateTime expires)> CreateAccessTokenAsync(AppUser user)
        {
            var userRoles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Name, user.UserName ?? user.Id)
            };

            claims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

            var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key not configured");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expires = DateTime.UtcNow.AddHours(1);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds);

            return (new JwtSecurityTokenHandler().WriteToken(token), expires);
        }

        private static DateTime ToMoscowTime(DateTime utcDateTime)
        {
            var moscowZone = TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, moscowZone);
        }
    }
}