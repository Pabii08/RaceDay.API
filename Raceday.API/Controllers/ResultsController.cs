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

        // POST: api/results - Organiser only
        [HttpPost]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> CaptureResult([FromBody] Result result)
        {
            _context.Results.Add(result);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Result captured successfully.", result });
        }

        // GET: api/results/my-results - Participant only
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

        // GET: api/results/event/5 - Both roles can view
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