using System.ComponentModel.DataAnnotations;

namespace JwtApp.Models
{
    public class DentalCase
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        [StringLength(200)]
        public string CaseName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public string ImageUrl { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
