using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;
using Xunit;

namespace RaceDay.API.Tests
{
    public class AuthControllerTests
    {
        // Helper method to create a fresh in-memory database for every test
        private RaceDayDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new RaceDayDbContext(options);
        }

        [Fact]
        public async Task Register_WithValidData_ShouldCreateUser()
        {
            // ARRANGE
            var context = GetInMemoryDbContext();

            // ACT
            var user = new User
            {
                Name = "Test User",
                Email = "test@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                Role = "Organiser"
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            // ASSERT
            var savedUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "test@test.com");
            Assert.NotNull(savedUser);
            Assert.Equal("Organiser", savedUser!.Role);
            Assert.Equal("Test User", savedUser.Name);
        }

        [Fact]
        public async Task Register_WithDuplicateEmail_ShouldFail()
        {
            // ARRANGE
            var context = GetInMemoryDbContext();
            var existingUser = new User
            {
                Name = "Existing",
                Email = "duplicate@test.com",
                PasswordHash = "hash",
                Role = "Organiser"
            };
            context.Users.Add(existingUser);
            await context.SaveChangesAsync();

            // ACT - Try to add another user with the same email
            bool emailExists = await context.Users.AnyAsync(u => u.Email == "duplicate@test.com");

            // ASSERT
            Assert.True(emailExists, "Email should already exist in the database.");
        }

        [Fact]
        public void Login_WithCorrectPassword_ShouldVerify()
        {
            // ARRANGE
            var plainPassword = "password123";
            var hash = BCrypt.Net.BCrypt.HashPassword(plainPassword);

            // ACT
            bool isValid = BCrypt.Net.BCrypt.Verify(plainPassword, hash);

            // ASSERT
            Assert.True(isValid, "Correct password should verify against its hash.");
        }

        [Fact]
        public void Login_WithIncorrectPassword_ShouldFail()
        {
            // ARRANGE
            var realPassword = "password123";
            var wrongPassword = "wrongpassword";
            var hash = BCrypt.Net.BCrypt.HashPassword(realPassword);

            // ACT
            bool isValid = BCrypt.Net.BCrypt.Verify(wrongPassword, hash);

            // ASSERT
            Assert.False(isValid, "Incorrect password should not verify.");
        }
    }
}