// Program.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ConstructionCompany.API.Data;
using ConstructionCompany.API.Models;

var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// НАСТРОЙКА БАЗЫ ДАННЫХ И IDENTITY
// ============================================================================

// 1. Подключение к SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Настройка Identity системы (управление пользователями)
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    // Настройки пароля для безопасности
    options.Password.RequireDigit = true;          // Требуются цифры
    options.Password.RequireLowercase = true;      // Требуются строчные буквы
    options.Password.RequireUppercase = false;     // Заглавные буквы не обязательны
    options.Password.RequireNonAlphanumeric = false; // Спецсимволы не обязательны
    options.Password.RequiredLength = 6;           // Минимальная длина 6 символов
    
    // Настройки пользователя
    options.User.RequireUniqueEmail = true;        // Уникальный email для каждого пользователя
})
.AddEntityFrameworkStores<ApplicationDbContext>()  // Хранение в базе данных
.AddDefaultTokenProviders();                       // Генерация токенов

// ============================================================================
// НАСТРОЙКА JWT АУТЕНТИФИКАЦИИ
// ============================================================================

// 3. Добавляем JWT аутентификацию
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,                     // Проверять издателя токена
        ValidateAudience = true,                   // Проверять получателя токена
        ValidateLifetime = true,                   // Проверять срок действия токена
        ValidateIssuerSigningKey = true,           // Проверять ключ подписи
        
        // Данные из appsettings.json
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});

// ============================================================================
// НАСТРОЙКА CORS ДЛЯ ANGULAR
// ============================================================================

// 4. Настройка CORS (Cross-Origin Resource Sharing) для доступа с разных портов
// 4. Настройка CORS (Cross-Origin Resource Sharing) для доступа с разных портов
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        // Разрешаем доступ с этих адресов Angular приложения
        policy.WithOrigins(
            "http://localhost:4200",     // Стандартный порт Angular
            "http://localhost:56867",    // Порт который был ранее
            "http://localhost:50797",    // Текущий порт Angular
            "http://localhost:5248"      // ← ДОБАВЬ ЭТОТ ПОРТ! Текущий порт бекенда
        )
        .AllowAnyHeader()                // Разрешаем любые заголовки
        .AllowAnyMethod()                // Разрешаем любые HTTP методы
        .AllowCredentials();             // ← ДОБАВЬ ЭТУ СТРОЧКУ!
    });
});

// ============================================================================
// ОСТАЛЬНЫЕ СЕРВИСЫ
// ============================================================================

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ============================================================================
// КОНФИГУРАЦИЯ PIPELINE (ПОСЛЕДОВАТЕЛЬНОСТЬ MIDDLEWARE)
// ============================================================================

// ВАЖНО: CORS должен быть первым в цепочке middleware
app.UseCors("AllowAngular");

// ВАЖНО: Аутентификация ДО авторизации
app.UseAuthentication();  // Проверка кто пользователь (JWT токен)
app.UseAuthorization();   // Проверка что может делать пользователь (роли)

// Swagger только в режиме разработки
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

// ============================================================================
// ИНИЦИАЛИЗАЦИЯ БАЗЫ ДАННЫХ ПРИ ЗАПУСКЕ
// ============================================================================

//using (var scope = app.Services.CreateScope())
//{
   // var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
  //  var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
   // var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    
  //  await DbInitializer.InitializeAsync(context, userManager, roleManager);
//}

app.Run();

