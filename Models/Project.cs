// Models/Project.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConstructionCompany.API.Models
{
    /// <summary>
    /// Модель типового проекта дома
    /// Шаблоны которые видит клиент в каталоге
    /// </summary>
    public class Project
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty; // "Дом Альфа", "Коттедж Бета"
        
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [Column(TypeName = "decimal(18,2)")]  // Указываем тип в SQL Server
        public decimal Price { get; set; }
        
        [MaxLength(200)]
        public string ImageUrl { get; set; } = string.Empty;
        
        [MaxLength(200)]
        public string? PlanUrl { get; set; } // План дома (опционально)
        
        [MaxLength(200)]
        public string Specifications { get; set; } = string.Empty; // "120м², 2 этажа"
        
        public int Area { get; set; }        // Площадь в м²
        public int Bedrooms { get; set; }    // Количество спален
        public int Bathrooms { get; set; }   // Количество санузлов
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}