using JwtApp.Data;
using JwtApp.DTO;
using JwtApp.Models;
using JwtApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JwtApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class CaseController : ControllerBase
    {
        private readonly JWTDbContext _context;
        private readonly ILogger<CaseController> _logger;

        public CaseController(JWTDbContext context, ILogger<CaseController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Create a new dental case
        /// </summary>
        /// <param name="caseData">Case information</param>
        /// <returns>Created case information</returns>
        [HttpPost]
        public async Task<IActionResult> CreateCase([FromBody] CreateCaseDTO caseData)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Create dental case
                var dentalCase = new DentalCase
                {
                    Id = Guid.NewGuid(),
                    CaseName = caseData.CaseName,
                    Description = caseData.Description,
                    ImageUrl = caseData.ImageUrl,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.DentalCases.Add(dentalCase);
                await _context.SaveChangesAsync();

                var response = new CaseResponseDTO
                {
                    Id = dentalCase.Id,
                    CaseName = dentalCase.CaseName,
                    Description = dentalCase.Description,
                    ImageUrl = dentalCase.ImageUrl,
                    CreatedAt = dentalCase.CreatedAt,
                    UpdatedAt = dentalCase.UpdatedAt
                };

                _logger.LogInformation($"Dental case created successfully: {dentalCase.Id}");

                return Ok(new
                {
                    Message = "Dental case created successfully",
                    Case = response
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating dental case");
                return StatusCode(500, new { Message = "An error occurred while creating the case" });
            }
        }

        /// <summary>
        /// Get all dental cases
        /// </summary>
        /// <returns>List of all dental cases</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CaseResponseDTO>>> GetAllCases()
        {
            try
            {
                var cases = await _context.DentalCases
                    .OrderByDescending(c => c.CreatedAt)
                    .Select(c => new CaseResponseDTO
                    {
                        Id = c.Id,
                        CaseName = c.CaseName,
                        Description = c.Description,
                        ImageUrl = c.ImageUrl,
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt
                    })
                    .ToListAsync();

                return Ok(cases);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dental cases");
                return StatusCode(500, new { Message = "An error occurred while retrieving cases" });
            }
        }

        /// <summary>
        /// Get a specific dental case by ID
        /// </summary>
        /// <param name="id">Case ID</param>
        /// <returns>Dental case information</returns>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<CaseResponseDTO>> GetCaseById(Guid id)
        {
            try
            {
                var dentalCase = await _context.DentalCases
                    .Where(c => c.Id == id)
                    .Select(c => new CaseResponseDTO
                    {
                        Id = c.Id,
                        CaseName = c.CaseName,
                        Description = c.Description,
                        ImageUrl = c.ImageUrl,
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt
                    })
                    .FirstOrDefaultAsync();

                if (dentalCase == null)
                    return NotFound(new { Message = "Dental case not found" });

                return Ok(dentalCase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving dental case: {id}");
                return StatusCode(500, new { Message = "An error occurred while retrieving the case" });
            }
        }

        /// <summary>
        /// Delete a dental case
        /// </summary>
        /// <param name="id">Case ID</param>
        /// <returns>Success message</returns>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteCase(Guid id)
        {
            try
            {
                var dentalCase = await _context.DentalCases.FindAsync(id);
                if (dentalCase == null)
                    return NotFound(new { Message = "Dental case not found" });

                // Remove from database
                _context.DentalCases.Remove(dentalCase);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Dental case deleted successfully: {id}");

                return Ok(new { Message = "Dental case deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting dental case: {id}");
                return StatusCode(500, new { Message = "An error occurred while deleting the case" });
            }
        }
    }
}
