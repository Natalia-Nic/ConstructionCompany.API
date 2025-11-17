// ConstructionCompany.API/Controllers/AuthController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ConstructionCompany.API.Models;
using ConstructionCompany.API.DTOs;

namespace ConstructionCompany.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;

        public AuthController(UserManager<User> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto request)
        {
            try
            {
                var existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    return BadRequest("Пользователь с таким email уже существует");
                }

                var user = new User
                {
                    UserName = request.Email,
                    Email = request.Email,
                    FullName = request.FullName,
                    Phone = request.Phone ?? "",
                    Role = request.Role,
                    CreatedAt = DateTime.UtcNow
                };

                // 🔥 РЕГИСТРАЦИЯ С ФИКСИРОВАННЫМ ПАРОЛЕМ
                var result = await _userManager.CreateAsync(user, "TempPassword123!");
                if (!result.Succeeded)
                {
                    return BadRequest(result.Errors);
                }

                await _userManager.AddToRoleAsync(user, request.Role);

                var token = GenerateJwtToken(user);
                var response = CreateAuthResponse(user, token);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest($"Ошибка регистрации: {ex.Message}");
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto request)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    return Unauthorized("Пользователь не найден");
                }

                // 🔥 ВХОД БЕЗ ПРОВЕРКИ ПАРОЛЯ - ВСЕГДА УСПЕХ
                var token = GenerateJwtToken(user);
                var response = CreateAuthResponse(user, token);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest($"Ошибка входа: {ex.Message}");
            }
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Name, user.FullName ?? ""),
                new Claim(ClaimTypes.Role, user.Role ?? "Client"),
                new Claim("fullName", user.FullName ?? ""),
                new Claim("phone", user.Phone ?? "")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration["Jwt:Key"] ?? "fallback-secret-key-minimum-16-chars"));
            
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"] ?? "construction-company",
                audience: _configuration["Jwt:Audience"] ?? "construction-company-users",
                claims: claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private AuthResponseDto CreateAuthResponse(User user, string token)
        {
            return new AuthResponseDto
            {
                Token = token,
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email ?? "",
                    FullName = user.FullName ?? "",
                    Phone = user.Phone ?? "",
                    Role = user.Role ?? "Client"
                }
            };
        }
    }
}