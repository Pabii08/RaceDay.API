using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Filters;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public CategoriesController(RaceDayDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all categories for a specific event.
        /// </summary>
        /// <param name="eventId">The ID of the event.</param>
        /// <returns>A list of categories for that event. Requires any authenticated user.</returns>
        [HttpGet("event/{eventId}")]
        [SessionAuthorize]
        public async Task<IActionResult> GetCategoriesForEvent(int eventId)
        {
            var categories = await _context.Categories
                .Where(c => c.EventId == eventId)
                .ToListAsync();
            return Ok(categories);
        }

        /// <summary>
        /// Creates a new category for an event. Only accessible to Organisers.
        /// </summary>
        /// <param name="category">The category details including name and event ID.</param>
        /// <returns>A confirmation message with the created category. Returns 403 for Participants.</returns>
        [HttpPost]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> CreateCategory([FromBody] Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Category created successfully.", category });
        }

        /// <summary>
        /// Deletes a category. Only accessible to Organisers.
        /// </summary>
        /// <param name="id">The ID of the category to delete.</param>
        /// <returns>A confirmation message. Returns 404 if not found.</returns>
        [HttpDelete("{id}")]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound(new { message = "Category not found." });

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Category deleted successfully." });
        }
    }
}