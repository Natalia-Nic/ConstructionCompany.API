// Controllers/ApplicationsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using ConstructionCompany.API.Data;
using ConstructionCompany.API.Models;
using System.Security.Claims;

namespace ConstructionCompany.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ApplicationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public ApplicationsController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/applications - все заявки (для подрядчика)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Application>>> GetAllApplications()
        {
            var applications = await _context.Applications
                .Include(a => a.Project)
                .Include(a => a.Client)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return applications;
        }

        // GET: api/applications/my - заявки текущего пользователя
        [HttpGet("my")]
        public async Task<ActionResult<IEnumerable<Application>>> GetMyApplications()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var applications = await _context.Applications
                .Include(a => a.Project)
                .Where(a => a.ClientId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return applications;
        }

        // POST: api/applications - создать заявку
        [HttpPost]
        public async Task<ActionResult<Application>> CreateApplication([FromBody] CreateApplicationRequest request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("Пользователь не авторизован");

                var project = await _context.Projects.FindAsync(request.ProjectId);
                if (project == null)
                    return BadRequest("Проект не найден");

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return BadRequest("Пользователь не найден");

                var application = new Application
                {
                    ClientId = userId,
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
                .Include(a => a.Client)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (application == null) return NotFound();

            return application;
        }

        // PUT: api/applications/5 - обновить заявку (для подрядчика)
        [HttpPut("{id}")]
        public async Task<ActionResult<Application>> UpdateApplication(int id, [FromBody] Application updates)
        {
            var application = await _context.Applications
                .Include(a => a.Project)
                .Include(a => a.Client)
                .FirstOrDefaultAsync(a => a.Id == id);
                
            if (application == null) return NotFound();

            if (!string.IsNullOrEmpty(updates.Status))
                application.Status = updates.Status;
            
            if (!string.IsNullOrEmpty(updates.ContractorComments))
                application.ContractorComments = updates.ContractorComments;

            application.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return application;
        }

        // PUT: api/applications/5/status - сменить статус
        [HttpPut("{id}/status")]
        public async Task<ActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
        {
            var application = await _context.Applications
                .Include(a => a.Project)
                .Include(a => a.Client)
                .FirstOrDefaultAsync(a => a.Id == id);
                
            if (application == null) 
                return NotFound("Заявка не найдена");

            application.Status = request.NewStatus;
            application.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            
            return Ok(new { 
                message = "Статус обновлен", 
                status = request.NewStatus,
                applicationId = id 
            });
        }
    }

    public class CreateApplicationRequest
    {
        public int ProjectId { get; set; }
        public string? ClientComments { get; set; }
    }

    public class UpdateStatusRequest
    {
        public string NewStatus { get; set; } = string.Empty;
    }
}