using System;
using System.ComponentModel.DataAnnotations;

namespace JwtApp.DTO
{
    public class CreateAppointmentDTO
    {
        [Required]
        public string PatientName { get; set; } = string.Empty;

        [Required]
        public DateTime AppointmentDate { get; set; }
    }
}