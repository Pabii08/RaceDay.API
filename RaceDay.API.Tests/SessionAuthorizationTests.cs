using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using RaceDay.API.Filters;
using Xunit;

namespace RaceDay.API.Tests
{
    public class SessionAuthorizationTests
    {
        // Helper: creates a fake ActionExecutingContext with a real session
        private ActionExecutingContext CreateActionContext(ISession session)
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Session = session;

            var actionContext = new ActionContext(
                httpContext,
                new RouteData(),
                new ActionDescriptor()
            );

            return new ActionExecutingContext(
                actionContext,
                new List<IFilterMetadata>(),
                new Dictionary<string, object?>(),
                controller: null!
            );
        }

        // Helper: creates a real in-memory session
        private ISession CreateSession()
        {
            return new TestSession();
        }

        [Fact]
        public void SessionAuthorize_WithNoLogin_ShouldReturn401()
        {
            // ARRANGE: No session data set (simulates unauthenticated user)
            var session = CreateSession();
            var context = CreateActionContext(session);
            var attribute = new SessionAuthorizeAttribute();

            // ACT
            attribute.OnActionExecuting(context);

            // ASSERT
            Assert.NotNull(context.Result);
            var result = context.Result as UnauthorizedObjectResult;
            Assert.NotNull(result);
            Assert.Equal(401, result!.StatusCode);
        }

        [Fact]
        public void SessionAuthorize_WithWrongRole_ShouldReturn403()
        {
            // ARRANGE: Logged in as Participant, but endpoint requires Organiser
            var session = CreateSession();
            session.SetInt32("UserId", 1);
            session.SetString("UserRole", "Participant");

            var context = CreateActionContext(session);
            var attribute = new SessionAuthorizeAttribute("Organiser");

            // ACT
            attribute.OnActionExecuting(context);

            // ASSERT
            Assert.NotNull(context.Result);
            var result = context.Result as ObjectResult;
            Assert.NotNull(result);
            Assert.Equal(403, result!.StatusCode);
        }

        [Fact]
        public void SessionAuthorize_WithCorrectRole_ShouldPass()
        {
            // ARRANGE: Logged in as Organiser, endpoint requires Organiser
            var session = CreateSession();
            session.SetInt32("UserId", 1);
            session.SetString("UserRole", "Organiser");

            var context = CreateActionContext(session);
            var attribute = new SessionAuthorizeAttribute("Organiser");

            // ACT
            attribute.OnActionExecuting(context);

            // ASSERT: No rejection result should be set
            Assert.Null(context.Result);
        }

        [Fact]
        public void SessionAuthorize_WithNoRoleRequired_ShouldPassWhenLoggedIn()
        {
            // ARRANGE: Any logged-in user, no specific role required
            var session = CreateSession();
            session.SetInt32("UserId", 1);
            session.SetString("UserRole", "Participant");

            var context = CreateActionContext(session);
            var attribute = new SessionAuthorizeAttribute();

            // ACT
            attribute.OnActionExecuting(context);

            // ASSERT
            Assert.Null(context.Result);
        }

        [Fact]
        public void SessionAuthorize_ParticipantAccessingParticipantEndpoint_ShouldPass()
        {
            // ARRANGE: Logged in as Participant, endpoint requires Participant
            var session = CreateSession();
            session.SetInt32("UserId", 2);
            session.SetString("UserRole", "Participant");

            var context = CreateActionContext(session);
            var attribute = new SessionAuthorizeAttribute("Participant");

            // ACT
            attribute.OnActionExecuting(context);

            // ASSERT
            Assert.Null(context.Result);
        }

        [Fact]
        public void SessionAuthorize_OrganiserAccessingParticipantEndpoint_ShouldReturn403()
        {
            // ARRANGE: Logged in as Organiser, endpoint requires Participant
            var session = CreateSession();
            session.SetInt32("UserId", 1);
            session.SetString("UserRole", "Organiser");

            var context = CreateActionContext(session);
            var attribute = new SessionAuthorizeAttribute("Participant");

            // ACT
            attribute.OnActionExecuting(context);

            // ASSERT
            Assert.NotNull(context.Result);
            var result = context.Result as ObjectResult;
            Assert.NotNull(result);
            Assert.Equal(403, result!.StatusCode);
        }
    }

    // Test implementation of ISession for unit tests
    public class TestSession : ISession
    {
        private readonly Dictionary<string, byte[]> _store = new();

        public bool IsAvailable => true;
        public string Id => Guid.NewGuid().ToString();
        public IEnumerable<string> Keys => _store.Keys;

        public void Clear() => _store.Clear();
        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Remove(string key) => _store.Remove(key);
        public void Set(string key, byte[] value) => _store[key] = value;
        public bool TryGetValue(string key, out byte[] value) => _store.TryGetValue(key, out value!);
    }
}