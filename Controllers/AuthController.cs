// Controllers/AuthController.cs
using JwtApp.DTO;
using JwtApp.Models;
using JwtApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace JwtApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly UserManager<User> _userManager;

        public AuthController(IAuthService authService, UserManager<User> userManager)
        {
            _authService = authService;
            _userManager = userManager;
        }

        // --- User REGISTRATION ENDPOINT ---
        [HttpPost("register/user")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterUser([FromBody] UserRegisterDTO request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // --- Check for existing Email (Emails must be unique) ---
            var existingUserByEmail = await _userManager.FindByEmailAsync(request.Email);
            if (existingUserByEmail != null)
                return BadRequest(new { Message = $"Email '{request.Email}' is already registered." });

            // --- FIX: The check for a unique username has been removed as per your request ---
            // The database schema change now handles this rule automatically.

            var user = new User
            {
                Id = Guid.NewGuid(),
                UserName = request.UserName,
                Email = request.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
                return BadRequest(new { Message = "User registration failed.", Errors = result.Errors.Select(e => e.Description) });

            var roleResult = await _userManager.AddToRoleAsync(user, "User");
            if (!roleResult.Succeeded)
                return BadRequest(new { Message = $"User '{request.UserName}' created, but failed to assign 'User' role.", Errors = roleResult.Errors.Select(e => e.Description) });

            return Ok(new { Message = $"User '{user.UserName}' registered successfully.", UserId = user.Id });
        }



        // ---  Login Endpoint  ---
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<TokenResponseDTO>> Login([FromBody] LoginDTO request)
        {
            var tokenResponse = await _authService.LoginAsync(request);
            if (tokenResponse is null)
            {
                return BadRequest("Invalid email or password.");
            }
            return Ok(tokenResponse);
        }

        // --- Refresh Token ---
        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<ActionResult<TokenResponseDTO>> RefreshToken(RefreshTokenRequestDTO request)
        {
            var tokenResponse = await _authService.RefreshTokenAsync(request);
            if (tokenResponse is null || string.IsNullOrEmpty(tokenResponse.AccessToken) || string.IsNullOrEmpty(tokenResponse.RefreshToken))
                return BadRequest("Invalid client request or refresh token expired.");
            return Ok(tokenResponse);
        }

        // --- Test Endpoints ---
        [HttpGet("TestAuthentication")]
        [Authorize]
        public ActionResult<object> GetMe()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = User.FindFirstValue(ClaimTypes.Name);
            var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            if (userId == null || userName == null)
            {
                return Unauthorized("Could not identify user from token.");
            }

            return Ok(new { UserId = userId, UserName = userName, Roles = roles });
        }

        [HttpGet("TestAdminAuthorization")]
        [Authorize(Roles = "Admin")]
        public ActionResult<string> GetAdminTest()
        {
            var userName = User.FindFirstValue(ClaimTypes.Name);
            return $"Hello Admin '{userName}'!";
        }
    }
}