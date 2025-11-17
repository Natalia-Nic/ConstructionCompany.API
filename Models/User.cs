using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ConstructionCompany.API.Models
{
    public class User : IdentityUser
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;
        
        [MaxLength(20)]
        public string? Phone { get; set; }
        
        [MaxLength(20)]
        public string Role { get; set; } = "Client"; // Admin, Client, Contractor
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}