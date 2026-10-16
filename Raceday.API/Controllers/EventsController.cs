using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Filters;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EventsController(RaceDayDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all events in the system, including their categories.
        /// </summary>
        /// <returns>A list of all events. Requires any authenticated user.</returns>
        [HttpGet]
        [SessionAuthorize]
        public async Task<IActionResult> GetAllEvents()
        {
            var events = await _context.Events
                .Include(e => e.Categories)
                .ToListAsync();

            return Ok(events);
        }

        /// <summary>
        /// Retrieves a single event by its ID, including its categories.
        /// </summary>
        /// <param name="id">The unique ID of the event.</param>
        /// <returns>The event details, or 404 if not found.</returns>
        [HttpGet("{id}")]
        [SessionAuthorize]
        public async Task<IActionResult> GetEventById(int id)
        {
            var eventItem = await _context.Events
                .Include(e => e.Categories)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (eventItem == null)
            {
                return NotFound(new { message = "Event not found." });
            }

            return Ok(eventItem);
        }

        /// <summary>
        /// Creates a new event. Only accessible to Organisers.
        /// </summary>
        /// <param name="newEvent">The event details including name, description, date, location, distance, and event type.</param>
        /// <returns>The created event with a 201 Created response. Returns 403 for Participants.</returns>
        [HttpPost]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> CreateEvent([FromBody] Event newEvent)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEventById), new { id = newEvent.EventId }, newEvent);
        }

        /// <summary>
        /// Updates an existing event. Only accessible to Organisers.
        /// </summary>
        /// <param name="id">The ID of the event to update.</param>
        /// <param name="updatedEvent">The updated event details.</param>
        /// <returns>A confirmation message with the updated event. Returns 404 if not found.</returns>
        [HttpPut("{id}")]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] Event updatedEvent)
        {
            if (id != updatedEvent.EventId)
            {
                return BadRequest(new { message = "Event ID mismatch." });
            }

            var existingEvent = await _context.Events.FindAsync(id);
            if (existingEvent == null)
            {
                return NotFound(new { message = "Event not found." });
            }

            existingEvent.Name = updatedEvent.Name;
            existingEvent.Description = updatedEvent.Description;
            existingEvent.Date = updatedEvent.Date;
            existingEvent.Location = updatedEvent.Location;
            existingEvent.Distance = updatedEvent.Distance;
            existingEvent.EventType = updatedEvent.EventType;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Event updated successfully.", eventItem = existingEvent });
        }

        /// <summary>
        /// Deletes an event. Only accessible to Organisers.
        /// </summary>
        /// <param name="id">The ID of the event to delete.</param>
        /// <returns>A confirmation message. Returns 404 if not found.</returns>
        [HttpDelete("{id}")]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var eventItem = await _context.Events.FindAsync(id);
            if (eventItem == null)
            {
                return NotFound(new { message = "Event not found." });
            }

            _context.Events.Remove(eventItem);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Event deleted successfully." });
        }
    }
}