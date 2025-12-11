using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using VolcanionAuth.Application.Common.Interfaces;
using System.Security.Claims;

namespace VolcanionAuth.API.Filters;

/// <summary>
/// Specifies that access to the decorated controller or action requires authorization for a specific resource and
/// action, using a custom policy evaluated at runtime.
/// </summary>
/// <remarks>Apply this attribute to controllers or actions to enforce fine-grained authorization based on the
/// current user's identity, route values, and query parameters. The attribute uses an authorization service to evaluate
/// whether the user is allowed to perform the specified action on the resource. If authorization fails, the request is
/// denied with an appropriate HTTP status code.</remarks>
/// <param name="resource">The name of the resource for which authorization is required. This value is passed to the authorization service to
/// determine access.</param>
/// <param name="action">The action to be performed on the resource that requires authorization. This value is used by the authorization
/// service to evaluate the policy.</param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequirePolicyAttribute(string resource, string action) : Attribute, IAsyncAuthorizationFilter
{
    /// <summary>
    /// Performs asynchronous authorization for the current HTTP request and sets the result on the provided filter
    /// context based on the authorization outcome.
    /// </summary>
    /// <remarks>If authorization fails, the method sets the result to an appropriate HTTP response, such as
    /// 401 Unauthorized or 403 Forbidden. If the authorization service is unavailable, a 500 Internal Server Error is
    /// returned. This method should be called within an ASP.NET Core authorization filter pipeline.</remarks>
    /// <param name="context">The filter context for the current authorization request. Must not be null. Contains information about the HTTP
    /// request, route data, and user principal.</param>
    /// <returns>A task that represents the asynchronous operation. The result of the authorization is set on the <paramref
    /// name="context"/>.</returns>
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        // Retrieve the authorization service from the dependency injection container
        var authService = context.HttpContext.RequestServices.GetService<IAuthorizationService>();
        if (authService == null)
        {
            // Authorization service is not available; return 500 Internal Server Error
            context.Result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
            return;
        }

        // Extract user ID from claims
        var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            // User is not authenticated or has no valid identifier; return 401 Unauthorized
            context.Result = new UnauthorizedResult();
            return;
        }

        // Build context from route values and query parameters
        var contextData = new Dictionary<string, object>();
        
        foreach (var routeValue in context.RouteData.Values)
        {
            // Only add non-null route values
            if (routeValue.Value != null)
            {
                // Convert route value to string
                contextData[routeValue.Key] = routeValue.Value;
            }    
        }

        // Add query parameters to context
        foreach (var queryParam in context.HttpContext.Request.Query)
        {
            // Use the first value of the query parameter
            contextData[queryParam.Key] = queryParam.Value.ToString() ?? "";
        }

        // Perform authorization check
        var result = await authService.AuthorizeAsync(userId, resource, action, contextData);
        if (!result.IsAllowed)
        {
            // Authorization failed; return 403 Forbidden with reason
            context.Result = new ObjectResult(new { error = result.Reason })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }
    }
}
