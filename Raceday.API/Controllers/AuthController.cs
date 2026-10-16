using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public AuthController(RaceDayDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Registers a new user account with the specified role.
        /// </summary>
        /// <param name="request">The registration details including name, email, password, and role (Organiser or Participant).</param>
        /// <returns>A confirmation message and the newly created user's details. Returns 400 if the email is already in use or the role is invalid.</returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto request)
        {
            // Check if email already exists
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return BadRequest(new { message = "Email already in use." });
            }

            // Validate the role
            if (request.Role != "Organiser" && request.Role != "Participant")
            {
                return BadRequest(new { message = "Role must be 'Organiser' or 'Participant'." });
            }

            // Hash the password using BCrypt
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                PasswordHash = passwordHash,
                Role = request.Role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var response = new UserResponseDto
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role
            };

            return Ok(new { message = "Registration successful", user = response });
        }

        /// <summary>
        /// Logs in a user and creates a server-side session.
        /// </summary>
        /// <param name="request">The user's email and password.</param>
        /// <returns>A confirmation message with the user's details. Stores session state for subsequent requests. Returns 401 if credentials are invalid.</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

            if (!isPasswordValid)
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            // Store user info in the session
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("UserRole", user.Role);
            HttpContext.Session.SetString("UserName", user.Name);

            var response = new UserResponseDto
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role
            };

            return Ok(new { message = "Login successful", user = response });
        }

        /// <summary>
        /// Logs out the current user and destroys their session.
        /// </summary>
        /// <returns>A confirmation message.</returns>
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Ok(new { message = "Logged out successfully." });
        }

        /// <summary>
        /// Returns the details of the currently logged-in user.
        /// </summary>
        /// <returns>The user's ID, name, and role. Returns 401 if not logged in.</returns>
        [HttpGet("me")]
        public IActionResult GetCurrentUser()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Unauthorized(new { message = "Not logged in." });
            }

            return Ok(new
            {
                UserId = userId,
                Name = HttpContext.Session.GetString("UserName"),
                Role = HttpContext.Session.GetString("UserRole")
            });
        }
    }
}