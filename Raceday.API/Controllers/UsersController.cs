using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Filters;

namespace RaceDay.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public UsersController(RaceDayDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves the profile of the currently logged-in user.
        /// </summary>
        /// <returns>The user's ID, name, email, and role. Returns 401 if not logged in.</returns>
        [HttpGet("profile")]
        [SessionAuthorize]
        public async Task<IActionResult> GetProfile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound(new { message = "User not found." });

            return Ok(new
            {
                user.UserId,
                user.Name,
                user.Email,
                user.Role
            });
        }

        /// <summary>
        /// Updates the profile of the currently logged-in user.
        /// </summary>
        /// <param name="request">The updated name and email.</param>
        /// <returns>A confirmation message. Returns 400 if the new email is already in use by another user.</returns>
        [HttpPut("profile")]
        [SessionAuthorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto request)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound(new { message = "User not found." });

            // Check if the new email is already taken by someone else
            var emailTaken = await _context.Users
                .AnyAsync(u => u.Email == request.Email && u.UserId != userId);

            if (emailTaken)
            {
                return BadRequest(new { message = "That email is already in use." });
            }

            user.Name = request.Name;
            user.Email = request.Email;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Profile updated successfully." });
        }
    }
}