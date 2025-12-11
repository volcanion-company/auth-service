using System.Security.Claims;
using VolcanionAuth.API.Services;

namespace VolcanionAuth.API.Middleware;

/// <summary>
/// Middleware that loads user-specific context and permissions for each HTTP request based on the authenticated user's
/// identifier.
/// </summary>
/// <remarks>This middleware should be placed early in the pipeline to ensure user context is available for
/// subsequent components. If the user identifier is missing or invalid, user context loading is skipped and the request
/// proceeds. Any exceptions during context loading are logged but do not interrupt request processing.</remarks>
/// <param name="next">The next middleware delegate in the HTTP request pipeline. Invoked after user context is loaded.</param>
/// <param name="logger">The logger used to record diagnostic and error information during user context loading.</param>
public class UserContextMiddleware(
    RequestDelegate next,
    ILogger<UserContextMiddleware> logger)
{
    /// <summary>
    /// Processes the incoming HTTP request and loads user-specific context information if a valid user identifier is
    /// present.
    /// </summary>
    /// <remarks>If the user is authenticated and a valid user identifier is found, user context and
    /// permissions are loaded before continuing request processing. If loading the user context fails, the request
    /// proceeds without additional user context.</remarks>
    /// <param name="context">The HTTP context for the current request. Provides access to user claims and request-specific data.</param>
    /// <param name="userContextService">A service used to load additional context and permissions for the authenticated user.</param>
    /// <returns>A task that represents the asynchronous operation of processing the HTTP request.</returns>
    public async Task InvokeAsync(HttpContext context, IUserContextService userContextService)
    {
        // Extract the user identifier from the claims principal
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var userGuid))
        {
            // Load the user context using the extracted user identifier
            try
            {
                // Load user permissions and additional context
                await userContextService.LoadUserContextAsync(userGuid, context.RequestAborted);
                // Log successful loading of user context
                logger.LogDebug("User context loaded for UserId={UserId}", userGuid);
            }
            catch (Exception ex)
            {
                // Log any exceptions that occur during user context loading
                logger.LogWarning(ex, "Failed to load user context for UserId={UserId}", userGuid);
            }
        }

        // Continue processing the HTTP request
        await next(context);
    }
}
