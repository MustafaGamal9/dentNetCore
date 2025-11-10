using JwtApp.Data;
using JwtApp.DTO;
using JwtApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
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

            // Validate that the preferred date is not in the past
            if (request.PreferredDate.Date < DateTime.Today)
            {
                return BadRequest(new { Message = "Preferred date cannot be in the past" });
            }

            // ✅ Extract UserId from JWT claims
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { Message = "User ID not found in token" });
            }

            var appointment = new Appointment
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ChildName = request.ChildName,
                ChildAge = request.ChildAge,
                ParentName = request.ParentName,
                Phone = request.Phone,
                Service = request.Service,
                PreferredDate = request.PreferredDate,
                Notes = request.Notes,
                CreatedAt = DateTime.UtcNow,
                IsConfirmed = false
            };

            await _context.Appointments.AddAsync(appointment);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Appointment request submitted successfully",
                AppointmentId = appointment.Id,
                ChildName = appointment.ChildName,
                PreferredDate = appointment.PreferredDate.ToString("yyyy-MM-dd"),
                Service = appointment.Service
            });
        }


        /// <summary>
        /// Allows the admin (dentist) to view all scheduled appointments.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAppointments()
        {
            var appointments = await _context.Appointments
                                             .OrderBy(a => a.PreferredDate)
                                             .ThenBy(a => a.CreatedAt)
                                             .ToListAsync();

            return Ok(appointments);
        }

        /// <summary>
        /// Allows a user to view all their own appointments.
        /// </summary>
        [HttpGet("my-appointments")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetUserAppointments()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(new { Message = "User ID could not be found in token." });
            }

            var userAppointments = await _context.Appointments
                                                 .Where(a => a.UserId == userId)
                                                 .OrderByDescending(a => a.CreatedAt)
                                                 .ToListAsync();

            if (!userAppointments.Any())
            {
                return Ok(new List<Appointment>());
            }

            return Ok(userAppointments);
        }


        /// <summary>
        /// Allows admin to confirm an appointment.
        /// </summary>
        [HttpPut("{id}/confirm")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ConfirmAppointment(Guid id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound(new { Message = "Appointment not found" });
            }

            appointment.IsConfirmed = true;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Appointment confirmed successfully",
                AppointmentId = appointment.Id,
                ChildName = appointment.ChildName,
                ConfirmedDate = appointment.PreferredDate.ToString("yyyy-MM-dd")
            });
        }

        /// <summary>
        /// Allows admin to cancel an appointment.
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CancelAppointment(Guid id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound(new { Message = "Appointment not found" });
            }

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Appointment cancelled successfully" });
        }
    }
}