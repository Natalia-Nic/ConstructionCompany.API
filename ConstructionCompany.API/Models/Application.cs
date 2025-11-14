// Models/Application.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConstructionCompany.API.Models
{
    public class Application
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ClientId { get; set; } = string.Empty;

        [Required]
        [ForeignKey("ClientId")] // ← ИСПРАВЬ НА "ClientId"!
        public User Client { get; set; } = null!;

        [Required]
        public int ProjectId { get; set; }

        [Required]
        [ForeignKey("ProjectId")] // ← ИСПРАВЬ НА "ProjectId"!
        public Project Project { get; set; } = null!;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending";

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public DateTime UpdatedAt { get; set; }

        [StringLength(500)]
        public string? ClientComments { get; set; }

        [StringLength(500)]
        public string? ContractorComments { get; set; }
    }

    // DTO для создания заявки
    public class CreateApplicationRequest
    {
        [Required]
        public int ProjectId { get; set; }

        [StringLength(500)]
        public string? ClientComments { get; set; }
    }
}