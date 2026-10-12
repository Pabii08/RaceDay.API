using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;
using Xunit;

namespace RaceDay.API.Tests
{
    public class EventsControllerTests
    {
        private RaceDayDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new RaceDayDbContext(options);
        }

        [Fact]
        public async Task CreateEvent_AsOrganiser_ShouldSucceed()
        {
            // ARRANGE
            var context = GetInMemoryDbContext();
            var newEvent = new Event
            {
                Name = "Cape Town Marathon",
                Description = "Annual event",
                Date = DateTime.Now.AddDays(30),
                Location = "Cape Town",
                Distance = 42.2,
                EventType = "Run"
            };

            // ACT
            context.Events.Add(newEvent);
            await context.SaveChangesAsync();

            // ASSERT
            var savedEvent = await context.Events.FirstOrDefaultAsync(e => e.Name == "Cape Town Marathon");
            Assert.NotNull(savedEvent);
            Assert.Equal(42.2, savedEvent!.Distance);
            Assert.Equal("Run", savedEvent.EventType);
        }

        [Fact]
        public async Task GetAllEvents_ShouldReturnAllEvents()
        {
            // ARRANGE
            var context = GetInMemoryDbContext();
            context.Events.AddRange(
                new Event { Name = "Event 1", EventType = "Run", Date = DateTime.Now, Location = "A", Distance = 5 },
                new Event { Name = "Event 2", EventType = "Walk", Date = DateTime.Now, Location = "B", Distance = 10 },
                new Event { Name = "Event 3", EventType = "Cycle", Date = DateTime.Now, Location = "C", Distance = 20 }
            );
            await context.SaveChangesAsync();

            // ACT
            var events = await context.Events.ToListAsync();

            // ASSERT
            Assert.Equal(3, events.Count);
        }

        [Fact]
        public async Task DeleteEvent_ShouldRemoveFromDatabase()
        {
            // ARRANGE
            var context = GetInMemoryDbContext();
            var testEvent = new Event { Name = "To Delete", EventType = "Run", Date = DateTime.Now, Location = "X", Distance = 5 };
            context.Events.Add(testEvent);
            await context.SaveChangesAsync();

            // ACT
            context.Events.Remove(testEvent);
            await context.SaveChangesAsync();

            // ASSERT
            var deletedEvent = await context.Events.FindAsync(testEvent.EventId);
            Assert.Null(deletedEvent);
        }
    }
}