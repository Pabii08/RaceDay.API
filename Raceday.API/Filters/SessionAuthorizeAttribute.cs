using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDay.API.Filters
{
    // This is our custom attribute. It can inherit from ActionFilterAttribute.
    public class SessionAuthorizeAttribute : ActionFilterAttribute
    {
        // We can pass in an optional role requirement, e.g. [SessionAuthorize("Organiser")]
        public string? RequiredRole { get; set; }

        public SessionAuthorizeAttribute() { }

        public SessionAuthorizeAttribute(string requiredRole)
        {
            RequiredRole = requiredRole;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // 1. Check if there is a UserId in the Session
            var userId = context.HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                // If not logged in, return 401 Unauthorized
                context.Result = new UnauthorizedObjectResult(new { message = "You must be logged in to access this resource." });
                return;
            }

            // 2. If a specific role is required, check it
            if (!string.IsNullOrEmpty(RequiredRole))
            {
                var userRole = context.HttpContext.Session.GetString("UserRole");

                if (userRole != RequiredRole)
                {
                    // If they are logged in but don't have the right role, return 403 Forbidden
                    context.Result = new ObjectResult(new { message = $"Access denied. This endpoint requires the '{RequiredRole}' role." })
                    {
                        StatusCode = 403
                    };
                    return;
                }
            }

            // 3. If everything passes, let the request continue
            base.OnActionExecuting(context);
        }
    }
}