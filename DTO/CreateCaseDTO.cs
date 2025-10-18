using System.ComponentModel.DataAnnotations;

namespace JwtApp.DTO
{
    public class CreateCaseDTO
    {
        [Required]
        [StringLength(200, ErrorMessage = "Case name cannot exceed 200 characters")]
        public string CaseName { get; set; } = string.Empty;

        [Required]
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string ImageUrl { get; set; } = string.Empty;
    }
}
