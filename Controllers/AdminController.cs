using JwtApp.DTO;
using JwtApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace JwtApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;

        public AdminController(UserManager<User> userManager, RoleManager<IdentityRole<Guid>> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // POST /api/Admin/create-user 
        [HttpPost("create-user")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDTO request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var roleName = request.Role;
            if (!await _roleManager.RoleExistsAsync(roleName))
                return BadRequest($"Invalid role specified: {roleName}. Role must be 'User' or 'Admin'.");

            // --- FIX: Removed the check for existing username ---

            var existingUserByEmail = await _userManager.FindByEmailAsync(request.Email);
            if (existingUserByEmail != null)
                return BadRequest(new { Message = $"Email '{request.Email}' is already registered." });

            var user = new User
            {
                Id = Guid.NewGuid(),
                UserName = request.Email, // Use email as username
                Email = request.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                EmailConfirmed = true,

            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
                return BadRequest(new { Message = "User creation failed.", Errors = result.Errors.Select(e => e.Description) });

            var roleResult = await _userManager.AddToRoleAsync(user, roleName);
            if (!roleResult.Succeeded)
                return BadRequest(new { Message = $"User '{request.Email}' created, but failed to assign role '{roleName}'.", Errors = roleResult.Errors.Select(e => e.Description) });

            return Ok(new
            {
                Message = $"CONFIRMED: User '{user.Email}' created successfully with role '{roleName}'.",
                UserId = user.Id,
                Email = user.Email,
                Role = roleName
            });
        }

        // --- GET ALL USERS  ---
        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<object>>> GetAllUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            var userResults = new List<object>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userResults.Add(new
                {
                    Id = user.Id,
                    Email = user.Email,


                });
            }
            return Ok(userResults);
        }


        [HttpGet("users/{id:guid}")]
        public async Task<ActionResult<object>> GetUserById(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return NotFound($"User with ID '{id}' not found.");

            var roles = await _userManager.GetRolesAsync(user);
            var userResult = new
            {
                Id = user.Id,
                Email = user.Email,
                Roles = roles,

            };
            return Ok(userResult);
        }

        // PUT /api/Admin/users/{id}
        [HttpPut("users/{id:guid}")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] CreateUserDTO request)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return NotFound($"User with ID '{id}' not found.");

            if (!await _roleManager.RoleExistsAsync(request.Role))
                return BadRequest($"Invalid role specified: {request.Role}. Role must be 'Admin' or 'User'.");

            // Use email as username
            await _userManager.SetUserNameAsync(user, request.Email);

            // Emails must be unique, so we check if the new email is taken by another user.
            if (user.Email != request.Email)
            {
                var existingUserByEmail = await _userManager.FindByEmailAsync(request.Email);
                if (existingUserByEmail != null && existingUserByEmail.Id != user.Id)
                    return BadRequest(new { Message = $"Email '{request.Email}' is already registered by another user." });
                await _userManager.SetEmailAsync(user, request.Email);
            }

            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                var passwordResult = await _userManager.ResetPasswordAsync(user, resetToken, request.Password);
                if (!passwordResult.Succeeded)
                {
                    return BadRequest(new { Message = $"Password update failed for user '{user.Email}'.", Errors = passwordResult.Errors.Select(e => e.Description) });
                }
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (!currentRoles.Contains(request.Role) || currentRoles.Count > 1)
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded)
                    return BadRequest(new { Message = $"Failed to remove existing roles for user '{user.Email}'.", Errors = removeResult.Errors.Select(e => e.Description) });

                var addResult = await _userManager.AddToRoleAsync(user, request.Role);
                if (!addResult.Succeeded)
                    return BadRequest(new { Message = $"Failed to assign new role '{request.Role}' for user '{user.Email}'.", Errors = addResult.Errors.Select(e => e.Description) });
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)

                return BadRequest(new { Message = $"Failed to save final updates for user '{user.Email}'.", Errors = updateResult.Errors.Select(e => e.Description) });

            return Ok(new { Message = $"CONFIRMED: User '{user.Email}' (ID: {id}) updated successfully." + (!string.IsNullOrWhiteSpace(request.Password) ? " Password was changed." : "") });
        }

        // --- DELETE USER ---
        [HttpDelete("users/{id:guid}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return NotFound($"User with ID '{id}' not found.");

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == id.ToString())
                return BadRequest("Administrators cannot delete their own account.");

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return BadRequest(new { Message = $"Failed to delete user '{user.Email}'.", Errors = result.Errors.Select(e => e.Description) });

            return Ok(new { Message = $"CONFIRMED: User '{user.Email}' (ID: {id}) deleted successfully." });
        }
    }
}