using System;
using System.ComponentModel.DataAnnotations;

namespace JwtApp.Models
{
    public class Appointment
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string PatientName { get; set; } = string.Empty;

        [Required]
        public DateTime AppointmentDate { get; set; }
    }
}