// Controllers/ApplicationsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ConstructionCompany.API.Data;
using ConstructionCompany.API.Models;

namespace ConstructionCompany.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApplicationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ApplicationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/applications - все заявки (для подрядчика) - ОБНОВЛЕННЫЙ МЕТОД!
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Application>>> GetAllApplications()
        {
            var applications = await _context.Applications
                .Include(a => a.Project)
                .Include(a => a.Client)
                .ToListAsync();

            return applications;
        }

        // GET: api/applications/my - заявки текущего пользователя
        [HttpGet("my")]
        public async Task<ActionResult<IEnumerable<Application>>> GetMyApplications()
        {
            // Временная реализация - возвращаем все заявки
            var applications = await _context.Applications
                .Include(a => a.Project)
                .ToListAsync();

            return applications;
        }

        // POST: api/applications - создать заявку
        [HttpPost]
        public async Task<ActionResult<Application>> CreateApplication([FromBody] CreateApplicationRequest request)
        {
            try
            {
                // Найдем первого пользователя в базе (тестового пользователя)
                var existingUser = await _context.Users.FirstOrDefaultAsync();
                if (existingUser == null)
                {
                    return BadRequest("No users found in database. Please create a test user first.");
                }

                // Создаем заявку с ID существующего пользователя
                var application = new Application
                {
                    ClientId = existingUser.Id,
                    ProjectId = request.ProjectId,
                    Status = "Pending",
                    ClientComments = request.ClientComments,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Applications.Add(application);
                await _context.SaveChangesAsync();

                return Ok(new { 
                    message = "Заявка успешно создана!", 
                    id = application.Id,
                    projectId = application.ProjectId
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Ошибка при создании заявки: {ex.Message}");
            }
        }

        // GET: api/applications/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Application>> GetApplication(int id)
        {
            var application = await _context.Applications
                .Include(a => a.Project)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (application == null) return NotFound();

            return application;
        }

        // PUT: api/applications/5 - обновить заявку (для подрядчика)
        [HttpPut("{id}")]
        public async Task<ActionResult<Application>> UpdateApplication(int id, [FromBody] Application updates)
        {
            var application = await _context.Applications.FindAsync(id);
            if (application == null) return NotFound();

            // Обновляем только разрешенные поля
            if (!string.IsNullOrEmpty(updates.Status))
                application.Status = updates.Status;
            
            if (!string.IsNullOrEmpty(updates.ContractorComments))
                application.ContractorComments = updates.ContractorComments;

            application.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return application;
        }
    }
}