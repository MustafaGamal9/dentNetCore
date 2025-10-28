using System;
using System.ComponentModel.DataAnnotations;

namespace JwtApp.DTO
{
    public class CreateAppointmentDTO
    {
        [Required(ErrorMessage = "Child name is required")]
        [StringLength(100, ErrorMessage = "Child name cannot exceed 100 characters")]
        public string ChildName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Child age is required")]
        [Range(0, 18, ErrorMessage = "Child age must be between 0 and 18 years")]
        public int ChildAge { get; set; }

        public string userId { get; set; }

        [Required(ErrorMessage = "Parent name is required")]
        [StringLength(100, ErrorMessage = "Parent name cannot exceed 100 characters")]
        public string ParentName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(15, ErrorMessage = "Phone number cannot exceed 15 characters")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Service type is required")]
        [StringLength(50, ErrorMessage = "Service type cannot exceed 50 characters")]
        public string Service { get; set; } = string.Empty;

        [Required(ErrorMessage = "Preferred date is required")]
        public DateTime PreferredDate { get; set; }

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }
    }
}