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

        // GET: api/categories/event/5 - Both roles can view
        [HttpGet("event/{eventId}")]
        [SessionAuthorize]
        public async Task<IActionResult> GetCategoriesForEvent(int eventId)
        {
            var categories = await _context.Categories
                .Where(c => c.EventId == eventId)
                .ToListAsync();
            return Ok(categories);
        }

        // POST: api/categories - Organiser only
        [HttpPost]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> CreateCategory([FromBody] Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Category created successfully.", category });
        }

        // DELETE: api/categories/5 - Organiser only
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