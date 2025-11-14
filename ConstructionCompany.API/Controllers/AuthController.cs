// Controllers/AuthController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ConstructionCompany.API.Data;
using ConstructionCompany.API.Models;

namespace ConstructionCompany.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;

        public AuthController(
            ApplicationDbContext context,
            UserManager<User> userManager,
            IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
        }

        // ============================================================================
        // РЕГИСТРАЦИЯ ПОЛЬЗОВАТЕЛЯ
        // ============================================================================
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
        {
            try
            {
                // Проверка существования пользователя
                var existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    return BadRequest("Пользователь с таким email уже существует");
                }

                // Создание нового пользователя
                var user = new User
                {
                    UserName = request.Email,
                    Email = request.Email ?? "",
                    FullName = request.FullName ?? "",
                    Phone = request.Phone ?? "",
                    Role = request.Role ?? "Client",
                    CreatedAt = DateTime.UtcNow
                };

                // Создание пользователя в Identity
                var result = await _userManager.CreateAsync(user, request.Password);

                if (!result.Succeeded)
                {
                    return BadRequest(result.Errors);
                }

                // Добавление роли пользователю
                await _userManager.AddToRoleAsync(user, request.Role ?? "Client");

                // Генерация JWT токена
                var token = GenerateJwtToken(user);

                // Возврат ответа
                var response = new AuthResponse
                {
                    Token = token,
                    User = new UserResponse
                    {
                        Id = user.Id,
                        Email = user.Email ?? "",
                        FullName = user.FullName ?? "",
                        Phone = user.Phone ?? "",
                        Role = user.Role ?? "Client"
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest($"Ошибка регистрации: {ex.Message}");
            }
        }

        // ============================================================================
        // ВХОД В СИСТЕМУ
        // ============================================================================
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
        {
            try
            {
                // Поиск пользователя по email
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    return Unauthorized("Неверный email или пароль");
                }

                // Проверка пароля
                var result = await _userManager.CheckPasswordAsync(user, request.Password);
                if (!result)
                {
                    return Unauthorized("Неверный email или пароль");
                }

                // Генерация JWT токена
                var token = GenerateJwtToken(user);

                // Возврат ответа
                var response = new AuthResponse
                {
                    Token = token,
                    User = new UserResponse
                    {
                        Id = user.Id,
                        Email = user.Email ?? "",
                        FullName = user.FullName ?? "",
                        Phone = user.Phone ?? "",
                        Role = user.Role ?? "Client"
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest($"Ошибка входа: {ex.Message}");
            }
        }

        // ============================================================================
        // ГЕНЕРАЦИЯ JWT ТОКЕНА
        // ============================================================================
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
    }

    // ============================================================================
    // DTO МОДЕЛИ ДЛЯ АУТЕНТИФИКАЦИИ
    // ============================================================================
    public class RegisterRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string Role { get; set; } = "Client";
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public UserResponse User { get; set; } = null!;
    }

    public class UserResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string Role { get; set; } = string.Empty;
    }
}