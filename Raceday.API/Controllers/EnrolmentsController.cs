using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Filters;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EnrolmentsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // POST: api/enrolments - Participant only
        [HttpPost]
        [SessionAuthorize("Participant")]
        public async Task<IActionResult> EnrolInEvent([FromBody] Enrolment enrolment)
        {
            // Get logged-in user's ID from session
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            enrolment.UserId = userId.Value;
            enrolment.EnrolmentDate = DateTime.Now;

            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Enrolled successfully.", enrolment });
        }

        // GET: api/enrolments/my-enrolments - Participant only
        [HttpGet("my-enrolments")]
        [SessionAuthorize("Participant")]
        public async Task<IActionResult> GetMyEnrolments()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var enrolments = await _context.Enrolments
                .Include(e => e.Event)
                .Include(e => e.Category)
                .Where(e => e.UserId == userId)
                .ToListAsync();
            return Ok(enrolments);
        }

        // GET: api/enrolments/event/5 - Organiser only
        [HttpGet("event/{eventId}")]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> GetEnrolmentsForEvent(int eventId)
        {
            var enrolments = await _context.Enrolments
                .Include(e => e.User)
                .Include(e => e.Category)
                .Where(e => e.EventId == eventId)
                .ToListAsync();
            return Ok(enrolments);
        }
    }
}