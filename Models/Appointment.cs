using System;
using System.ComponentModel.DataAnnotations;

namespace JwtApp.Models
{
    public class Appointment
    {
        [Key]
        public Guid Id { get; set; }
        public string UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string ChildName { get; set; } = string.Empty;

        [Required]
        [Range(0, 18, ErrorMessage = "Child age must be between 0 and 18 years")]
        public int ChildAge { get; set; }

        [Required]
        [StringLength(100)]
        public string ParentName { get; set; } = string.Empty;

        [Required]
        [Phone]
        [StringLength(15)]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Service { get; set; } = string.Empty;

        [Required]
        public DateTime PreferredDate { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsConfirmed { get; set; } = false;
    }
}