using FakeObsidian.Api.Models.User;
using FakeObsidian.Domain.Entities;
using FakeObsidian.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace FakeObsidian.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager,
        IConfiguration configuration, AppDbContext db) : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        private readonly IConfiguration _configuration = configuration;
        private readonly AppDbContext _db = db;

        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegRequest model)
        {
            if (await _userManager.FindByNameAsync(model.UserName) != null)
                return BadRequest("Пользователь уже существует");

            var user = new AppUser { UserName = model.UserName, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded) 
                return BadRequest(result.Errors);

            if (!await _roleManager.RoleExistsAsync("User"))
                await _roleManager.CreateAsync(new IdentityRole("User"));

            await _userManager.AddToRoleAsync(user, "User");
            var (accessToken, accessExp) = await CreateAccessTokenAsync(user);
            var refresh = CreateRefreshToken();
            refresh.UserId = user.Id;

            _db.RefreshTokens.Add(refresh);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                token = accessToken,
                expiration = accessExp,
                refreshToken = refresh.Token
            });
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LogRequest model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null) return Unauthorized("Неверный логин или пароль");

            if (!await _userManager.CheckPasswordAsync(user, model.Password))
                return Unauthorized("Неверный логин или пароль");

            var (accessToken, accessExp) = await CreateAccessTokenAsync(user);
            var refresh = CreateRefreshToken();
            refresh.UserId = user.Id;

            _db.RefreshTokens.Add(refresh);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                token = accessToken,
                expiration = accessExp,
                refreshToken = refresh.Token
            });
        }

        [HttpPost("refresh")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ValidationProblemDetails))]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
                return BadRequest();

            var existing = _db.RefreshTokens
                .FirstOrDefault(rt => rt.Token == request.RefreshToken);

            if (existing == null || !existing.IsActive)
                return Unauthorized();
            existing.Revoked = DateTime.UtcNow;

            var user = await _userManager.FindByIdAsync(existing.UserId);
            if (user == null) return Unauthorized();

            var (newAccessToken, newAccessExp) = await CreateAccessTokenAsync(user);
            var newRefresh = CreateRefreshToken();
            newRefresh.UserId = user.Id;

            _db.RefreshTokens.Add(newRefresh);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                token = newAccessToken,
                expiration = newAccessExp,
                refreshToken = newRefresh.Token
            });
        }

        private static RefreshToken CreateRefreshToken()
        {
            var randomBytes = RandomNumberGenerator.GetBytes(64);
            return new RefreshToken
            {
                Token = Convert.ToBase64String(randomBytes),
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow
            };
        }

        private async Task<(string token, DateTime expires)> CreateAccessTokenAsync(AppUser user)
        {
            var userRoles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.UserName ?? user.Id),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            claims.AddRange(userRoles.Select(r => new Claim(ClaimTypes.Role, r)));

            var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key not configured");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expires = DateTime.UtcNow.AddHours(1);
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return (new JwtSecurityTokenHandler().WriteToken(token), expires);
        }
    }
}
