using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using VolcanionAuth.Application.Common.Interfaces;
using System.Security.Claims;

namespace VolcanionAuth.API.Filters;

/// <summary>
/// Specifies that access to a controller or action requires a specific permission, defined by resource and action, for
/// the current user.
/// </summary>
/// <remarks>Apply this attribute to ASP.NET Core controllers or actions to enforce permission-based
/// authorization. The required permission is specified as a string in the format 'resource:action', such as
/// 'Document:Read'. During request processing, the attribute checks whether the authenticated user possesses the
/// specified permission using an authorization service. If the user lacks the required permission, the request is
/// denied with a 403 Forbidden response. If the user is not authenticated, a 401 Unauthorized response is returned. If
/// the authorization service is unavailable, a 500 Internal Server Error is returned.</remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequirePermissionAttribute : Attribute, IAsyncAuthorizationFilter
{
    /// <summary>
    /// Resource part of the required permission (e.g., "Document" in "Document:Read").
    /// </summary>
    private readonly string _resource;

    /// <summary>
    /// Action part of the required permission (e.g., "Read" in "Document:Read").
    /// </summary>
    private readonly string _action;

    /// <summary>
    /// Initializes a new instance of the RequirePermissionAttribute class using the specified permission string in
    /// 'resource:action' format.
    /// </summary>
    /// <remarks>The permission string must contain exactly one colon separating the resource and action. This
    /// attribute is typically used to enforce access control on methods or classes based on user permissions.</remarks>
    /// <param name="permissionString">A string representing the required permission, formatted as 'resource:action'. For example, 'user:read' or
    /// 'order:update'.</param>
    /// <exception cref="ArgumentException">Thrown if permissionString is null, empty, or not in the 'resource:action' format.</exception>
    public RequirePermissionAttribute(string permissionString)
    {
        // Validate and parse the permission string
        var parts = permissionString.Split(':');
        if (parts.Length != 2)
        {
            // Invalid format, throw an exception
            throw new ArgumentException("Permission must be in format 'resource:action'", nameof(permissionString));
        }

        // Assign resource and action
        _resource = parts[0];
        _action = parts[1];
    }

    /// <summary>
    /// Handles authorization for the current request by evaluating user permissions and setting the appropriate result
    /// on the context.
    /// </summary>
    /// <remarks>If the authorization service is unavailable, the method sets a 500 Internal Server Error
    /// result. If the user is not authenticated or lacks a valid identifier, an Unauthorized result is set. If the user
    /// does not have the required permission, a Forbid result is set. Otherwise, the request proceeds.</remarks>
    /// <param name="context">The filter context for the current authorization request. Provides access to the HTTP context, user claims, and
    /// a mechanism to set the result of the authorization process.</param>
    /// <returns>A task that represents the asynchronous operation. The result of the authorization is set on the provided
    /// context.</returns>
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        // Retrieve the authorization service from the request services
        var authService = context.HttpContext.RequestServices.GetService<IAuthorizationService>();
        if (authService == null)
        {
            // Authorization service is not available, return 500 Internal Server Error
            context.Result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
            return;
        }

        // Extract user ID from claims
        var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            // User is not authenticated or has no valid ID, return 401 Unauthorized
            context.Result = new UnauthorizedResult();
            return;
        }

        // Check if the user has the required permission
        var hasPermission = await authService.HasPermissionAsync(userId, _resource, _action);
        if (!hasPermission)
        {
            // User lacks the required permission, return 403 Forbidden
            context.Result = new ForbidResult();
        }
    }
}
