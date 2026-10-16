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

        /// <summary>
        /// Enrols the currently logged-in Participant in an event under a specific category.
        /// </summary>
        /// <param name="enrolment">The enrolment details including the event ID and category ID.</param>
        /// <returns>A confirmation message with the enrolment record. Returns 403 for Organisers.</returns>
        [HttpPost]
        [SessionAuthorize("Participant")]
        public async Task<IActionResult> EnrolInEvent([FromBody] Enrolment enrolment)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            enrolment.UserId = userId.Value;
            enrolment.EnrolmentDate = DateTime.Now;

            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Enrolled successfully.", enrolment });
        }

        /// <summary>
        /// Retrieves all enrolments for the currently logged-in Participant.
        /// </summary>
        /// <returns>A list of the participant's enrolments, including event and category details.</returns>
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

        /// <summary>
        /// Retrieves all enrolments for a specific event. Only accessible to Organisers.
        /// </summary>
        /// <param name="eventId">The ID of the event.</param>
        /// <returns>A list of all enrolments for that event, including participant details.</returns>
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