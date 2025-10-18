using JwtApp.Data;
using JwtApp.DTO;
using JwtApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace JwtApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AppointmentsController : ControllerBase
    {
        private readonly JWTDbContext _context;

        public AppointmentsController(JWTDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Allows a user to schedule a new appointment.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var appointment = new Appointment
            {
                Id = Guid.NewGuid(),
                PatientName = request.PatientName,
                AppointmentDate = request.AppointmentDate
            };

            await _context.Appointments.AddAsync(appointment);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Appointment created successfully", appointment.Id });
        }

        /// <summary>
        /// Allows the admin (dentist) to view all scheduled appointments.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAppointments()
        {
            var appointments = await _context.Appointments
                                             .OrderBy(a => a.AppointmentDate)
                                             .ToListAsync();

            return Ok(appointments);
        }
    }
}