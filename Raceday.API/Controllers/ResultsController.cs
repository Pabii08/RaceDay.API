using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Filters;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ResultsController(RaceDayDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Captures a result for a participant after an event. Only accessible to Organisers.
        /// </summary>
        /// <param name="result">The result details including enrolment ID, time, position, and status.</param>
        /// <returns>A confirmation message with the captured result.</returns>
        [HttpPost]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> CaptureResult([FromBody] Result result)
        {
            _context.Results.Add(result);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Result captured successfully.", result });
        }

        /// <summary>
        /// Retrieves all results for the currently logged-in Participant.
        /// </summary>
        /// <returns>A list of the participant's results. Returns 403 for Organisers.</returns>
        [HttpGet("my-results")]
        [SessionAuthorize("Participant")]
        public async Task<IActionResult> GetMyResults()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var results = await _context.Results
                .Include(r => r.Enrolment)
                .Where(r => r.Enrolment!.UserId == userId)
                .ToListAsync();
            return Ok(results);
        }

        /// <summary>
        /// Retrieves all results for a specific event, ordered by finishing position.
        /// </summary>
        /// <param name="eventId">The ID of the event.</param>
        /// <returns>A list of all results for that event. Requires any authenticated user.</returns>
        [HttpGet("event/{eventId}")]
        [SessionAuthorize]
        public async Task<IActionResult> GetResultsForEvent(int eventId)
        {
            var results = await _context.Results
                .Include(r => r.Enrolment)
                .Where(r => r.Enrolment!.EventId == eventId)
                .OrderBy(r => r.Position)
                .ToListAsync();
            return Ok(results);
        }
    }
}