using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;
using Xunit;

namespace RaceDay.API.Tests
{
    public class EnrolmentsControllerTests
    {
        private RaceDayDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new RaceDayDbContext(options);
        }

        [Fact]
        public async Task EnrolInEvent_AsParticipant_ShouldRecordEnrolment()
        {
            // ARRANGE
            var context = GetInMemoryDbContext();
            var participant = new User { Name = "P1", Email = "p1@test.com", PasswordHash = "x", Role = "Participant" };
            var testEvent = new Event { Name = "5K Run", EventType = "Run", Date = DateTime.Now, Location = "X", Distance = 5 };
            var category = new Category { Name = "Senior", Event = testEvent };
            context.Users.Add(participant);
            context.Events.Add(testEvent);
            context.Categories.Add(category);
            await context.SaveChangesAsync();

            // ACT
            var enrolment = new Enrolment
            {
                UserId = participant.UserId,
                EventId = testEvent.EventId,
                CategoryId = category.CategoryId,
                EnrolmentDate = DateTime.Now
            };
            context.Enrolments.Add(enrolment);
            await context.SaveChangesAsync();

            // ASSERT
            var savedEnrolment = await context.Enrolments
                .Include(e => e.User)
                .Include(e => e.Event)
                .FirstOrDefaultAsync(e => e.EnrolmentId == enrolment.EnrolmentId);

            Assert.NotNull(savedEnrolment);
            Assert.Equal("P1", savedEnrolment!.User!.Name);
            Assert.Equal("5K Run", savedEnrolment.Event!.Name);
        }

        [Fact]
        public async Task GetEnrolments_ForEvent_ShouldReturnAllParticipants()
        {
            // ARRANGE
            var context = GetInMemoryDbContext();
            var testEvent = new Event { Name = "Big Race", EventType = "Run", Date = DateTime.Now, Location = "X", Distance = 21 };
            context.Events.Add(testEvent);
            await context.SaveChangesAsync();

            // Add 3 fake enrolments
            for (int i = 1; i <= 3; i++)
            {
                var user = new User { Name = $"User{i}", Email = $"u{i}@test.com", PasswordHash = "x", Role = "Participant" };
                context.Users.Add(user);
                await context.SaveChangesAsync();

                var cat = new Category { Name = $"Cat{i}", EventId = testEvent.EventId };
                context.Categories.Add(cat);
                await context.SaveChangesAsync();

                context.Enrolments.Add(new Enrolment
                {
                    UserId = user.UserId,
                    EventId = testEvent.EventId,
                    CategoryId = cat.CategoryId,
                    EnrolmentDate = DateTime.Now
                });
            }
            await context.SaveChangesAsync();

            // ACT
            var enrolmentsForEvent = await context.Enrolments
                .Where(e => e.EventId == testEvent.EventId)
                .ToListAsync();

            // ASSERT
            Assert.Equal(3, enrolmentsForEvent.Count);
        }
    }
}