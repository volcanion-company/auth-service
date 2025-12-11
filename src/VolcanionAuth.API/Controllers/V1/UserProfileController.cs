using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VolcanionAuth.API.Attributes;
using VolcanionAuth.API.Extensions;
using VolcanionAuth.API.Services;

namespace VolcanionAuth.API.Controllers.V1;

/// <summary>
/// Provides API endpoints for retrieving and managing the current user's profile, roles, permissions, and custom
/// context data. All endpoints require authentication and operate on the authenticated user's information.
/// </summary>
/// <remarks>This controller exposes endpoints for accessing user profile details, roles, permissions, and custom
/// context data using various mechanisms, including extension methods, HTTP context, and attribute-based parameter
/// injection. All actions require the caller to be authenticated. The controller is versioned as part of the API and is
/// intended for use in scenarios where user-specific information must be retrieved or managed within the context of an
/// authenticated request.</remarks>
/// <param name="userContext">The service used to access information about the current user and manage user-specific context data.</param>
/// <param name="logger">The logger used to record diagnostic and operational information for the controller.</param>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class UserProfileController(
    IUserContextService userContext,
    ILogger<UserProfileController> logger) : ControllerBase
{
    /// <summary>
    /// Retrieves the profile information for the currently authenticated user.
    /// </summary>
    /// <remarks>The returned profile includes the user's ID, email address, first name, last name, roles, 
    /// all permissions granted through role assignments, and the current request identifier. 
    /// Permissions are returned in "resource:action" format (e.g., "users:read", "documents:write").
    /// This endpoint requires authentication.</remarks>
    /// <returns>An <see cref="IActionResult"/> containing the user's profile information if the user is authenticated;
    /// otherwise, an appropriate error response.</returns>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    public IActionResult GetMyProfile()
    {
        // Log the retrieval action
        logger.LogDebug("Retrieving profile for user ID: {UserId} with {PermissionCount} permissions", 
            userContext.UserId, userContext.Permissions.Count);
        
        // Method 1: Using extension methods
        var userId = this.GetCurrentUserId();
        var email = this.TryGetCurrentUserEmail();
        var roles = this.GetCurrentUserRoles();
        
        // Return the user profile response with all permissions
        return Ok(new UserProfileResponse
        {
            UserId = userId,
            Email = email ?? "N/A",
            FirstName = userContext.FirstName ?? "N/A",
            LastName = userContext.LastName ?? "N/A",
            Roles = roles,
            Permissions = userContext.Permissions, // All permissions from all assigned roles
            PermissionCount = userContext.Permissions.Count,
            RequestId = this.GetRequestId()
        });
    }

    /// <summary>
    /// Retrieves information about the currently authenticated user, including user ID, email address, roles, and the
    /// current request identifier.
    /// </summary>
    /// <remarks>This endpoint requires the user to be authenticated. The returned information reflects the
    /// claims present in the current HTTP context. If the user is not authenticated, the response may contain null or
    /// default values for user-related fields.</remarks>
    /// <returns>An <see cref="OkObjectResult"/> containing a <see cref="UserContextResponse"/> with details about the
    /// authenticated user. Returns HTTP 200 (OK) if the user context is successfully retrieved.</returns>
    [HttpGet("context")]
    [ProducesResponseType(typeof(UserContextResponse), StatusCodes.Status200OK)]
    public IActionResult GetUserContext()
    {
        // Log the retrieval action
        logger.LogDebug("Retrieving user context from HttpContext for request ID: {RequestId}", this.GetRequestId());
        // Method 2: Using HttpContext extensions
        var userId = HttpContext.GetUserId();
        var email = HttpContext.GetUserEmail();
        var roles = HttpContext.GetUserRoles();
        var requestId = HttpContext.GetRequestId();
        // Return the user context response
        return Ok(new UserContextResponse
        {
            UserId = userId,
            Email = email,
            Roles = roles,
            RequestId = requestId
        });
    }

    /// <summary>
    /// Retrieves information about the currently authenticated user.
    /// </summary>
    /// <remarks>This endpoint requires authentication. The user information is injected via the
    /// <c>[CurrentUser]</c> attribute and reflects the identity of the caller making the request.</remarks>
    /// <param name="currentUser">The current user's information, automatically provided by the authentication system. Cannot be null.</param>
    /// <returns>An <see cref="OkObjectResult"/> containing the current user's information with a status code of 200 (OK).</returns>
    [HttpGet("info")]
    [ProducesResponseType(typeof(CurrentUserInfo), StatusCodes.Status200OK)]
    public IActionResult GetUserInfo([CurrentUser] CurrentUserInfo currentUser)
    {
        // Log the retrieval action
        logger.LogDebug("Retrieving current user info for user ID: {UserId}", currentUser.UserId);
        // Method 3: Using attribute injection
        return Ok(currentUser);
    }

    /// <summary>
    /// Retrieves the permissions and roles assigned to the currently authenticated user.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> containing a <see cref="UserPermissionsResponse"/> with the user's ID, email,
    /// roles, permissions, and related access flags.</returns>
    [HttpGet("permissions")]
    [ProducesResponseType(typeof(UserPermissionsResponse), StatusCodes.Status200OK)]
    public IActionResult GetMyPermissions()
    {
        // Method 4: Using IUserContextService
        return Ok(new UserPermissionsResponse
        {
            UserId = userContext.UserId!.Value,
            Email = userContext.UserEmail ?? "N/A",
            Roles = userContext.Roles,
            Permissions = userContext.Permissions,
            HasAdminRole = userContext.HasRole("Admin"),
            CanCreateUsers = userContext.HasPermission("users:create")
        });
    }

    /// <summary>
    /// Stores custom key-value data in the current user's context.
    /// </summary>
    /// <param name="request">The custom context data to store, including the key and value to associate with the user context. Cannot be
    /// null.</param>
    /// <returns>An HTTP 200 OK response indicating that the custom context was stored successfully.</returns>
    [HttpPost("context/custom")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult SetCustomContext([FromBody] CustomContextRequest request)
    {
        // Log the storage action
        logger.LogDebug("Storing custom context data for user ID: {UserId}, Key: {Key}", userContext.UserId, request.Key);
        // Store custom data in user context
        userContext.SetCustomData(request.Key, request.Value);
        // Store in HttpContext
        HttpContext.SetUserContext(request.Key, request.Value);
        // Return success response
        return Ok(new { message = "Custom context stored successfully" });
    }

    /// <summary>
    /// Retrieves custom context data associated with the specified key from both the user context and the HTTP context.
    /// </summary>
    /// <param name="key">The key used to identify the custom context data to retrieve. Cannot be null.</param>
    /// <returns>An <see cref="IActionResult"/> containing a <see cref="CustomContextResponse"/> with the custom context data
    /// from both the user context and the HTTP context.</returns>
    [HttpGet("context/custom/{key}")]
    [ProducesResponseType(typeof(CustomContextResponse), StatusCodes.Status200OK)]
    public IActionResult GetCustomContext(string key)
    {
        // Log the retrieval action
        logger.LogDebug("Retrieving custom context data for user ID: {UserId}, Key: {Key}", userContext.UserId, key);
        // Retrieve custom data from user context
        var fromUserContext = userContext.GetCustomData<string>(key);
        // Retrieve custom data from HttpContext
        var fromHttpContext = HttpContext.GetUserContext<string>(key);
        // Return the custom context response
        return Ok(new CustomContextResponse
        {
            Key = key,
            FromUserContext = fromUserContext,
            FromHttpContext = fromHttpContext
        });
    }

    /// <summary>
    /// Checks whether the current user has the specified permission.
    /// </summary>
    /// <param name="permission">The name of the permission to check for the current user. Cannot be null or empty.</param>
    /// <returns>An <see cref="OkObjectResult"/> containing a <see cref="PermissionCheckResponse"/> that indicates whether the
    /// user has the specified permission.</returns>
    [HttpGet("check/permission/{permission}")]
    [ProducesResponseType(typeof(PermissionCheckResponse), StatusCodes.Status200OK)]
    public IActionResult CheckPermission(string permission)
    {
        // Log the permission check action
        logger.LogDebug("Checking permission '{Permission}' for user ID: {UserId}", permission, userContext.UserId);
        // Check if user has the specified permission
        var hasPermission = userContext.HasPermission(permission);
        // Return the permission check response
        return Ok(new PermissionCheckResponse
        {
            Permission = permission,
            HasPermission = hasPermission,
            UserId = userContext.UserId!.Value,
            AllPermissions = userContext.Permissions
        });
    }

    /// <summary>
    /// Checks whether the current user is assigned the specified role.
    /// </summary>
    /// <param name="role">The name of the role to check for the current user. Cannot be null or empty.</param>
    /// <returns>An <see cref="IActionResult"/> containing a <see cref="RoleCheckResponse"/> that indicates whether the user has
    /// the specified role.</returns>
    [HttpGet("check/role/{role}")]
    [ProducesResponseType(typeof(RoleCheckResponse), StatusCodes.Status200OK)]
    public IActionResult CheckRole(string role)
    {
        // Log the role check action
        logger.LogDebug("Checking role '{Role}' for user ID: {UserId}", role, userContext.UserId);
        // Check if user has the specified role
        var hasRole = userContext.HasRole(role);
        // Return the role check response
        return Ok(new RoleCheckResponse
        {
            Role = role,
            HasRole = hasRole,
            UserId = userContext.UserId!.Value,
            AllRoles = userContext.Roles
        });
    }
}

#region DTOs
/// <summary>
/// Represents the response containing user profile information, including user identity, email, roles, and permissions.
/// </summary>
/// <remarks>This data transfer object is typically used to return user profile details from authentication or
/// user management APIs. All properties are immutable and set during object initialization. Permissions are returned
/// in "resource:action" format (e.g., "users:read", "documents:write", "orders:approve").</remarks>
public record UserProfileResponse
{
    /// <summary>
    /// Gets the unique identifier for the user.
    /// </summary>
    public Guid UserId { get; init; }

    /// <summary>
    /// Gets the email address associated with the entity.
    /// </summary>
    public string Email { get; init; } = null!;

    /// <summary>
    /// Gets the first name of the person.
    /// </summary>
    public string FirstName { get; init; } = null!;

    /// <summary>
    /// Gets the last name of the person.
    /// </summary>
    public string LastName { get; init; } = null!;

    /// <summary>
    /// Gets the list of roles assigned to the user.
    /// </summary>
    public List<string> Roles { get; init; } = [];

    /// <summary>
    /// Gets the complete list of permissions associated with the user across all assigned roles.
    /// Each permission is formatted as "resource:action" (e.g., "users:read", "documents:write").
    /// Permissions are automatically aggregated from all active roles assigned to the user.
    /// </summary>
    public List<string> Permissions { get; init; } = [];

    /// <summary>
    /// Gets the total number of unique permissions assigned to the user.
    /// </summary>
    public int PermissionCount { get; init; }

    /// <summary>
    /// Gets the unique identifier for the request, if available.
    /// </summary>
    public string? RequestId { get; init; }
}

/// <summary>
/// Represents the user context information associated with a request, including user identity, email, roles, and
/// request identifier.
/// </summary>
public record UserContextResponse
{
    /// <summary>
    /// Gets the unique identifier of the user associated with this instance, if available.
    /// </summary>
    public Guid? UserId { get; init; }

    /// <summary>
    /// Gets the email address associated with the user or entity.
    /// </summary>
    public string? Email { get; init; }

    /// <summary>
    /// Gets the list of roles associated with the current entity.
    /// </summary>
    public List<string> Roles { get; init; } = [];

    /// <summary>
    /// Gets the unique identifier for the request, if available.
    /// </summary>
    public string? RequestId { get; init; }
}

/// <summary>
/// Represents the set of roles and permissions assigned to a user, including key authorization flags and identifying
/// information.
/// </summary>
/// <remarks>This record is typically used to convey a user's access rights and roles within an application, such
/// as in API responses or authorization checks. It includes both granular permissions and high-level role indicators to
/// support flexible access control scenarios.</remarks>
public record UserPermissionsResponse
{
    /// <summary>
    /// Gets the unique identifier for the user.
    /// </summary>
    public Guid UserId { get; init; }

    /// <summary>
    /// Gets the email address associated with the user.
    /// </summary>
    public string Email { get; init; } = null!;

    /// <summary>
    /// Gets the collection of roles assigned to the user.
    /// </summary>
    public List<string> Roles { get; init; } = [];

    /// <summary>
    /// Gets the list of permissions associated with the current instance.
    /// </summary>
    public List<string> Permissions { get; init; } = [];

    /// <summary>
    /// Gets a value indicating whether the user has the administrator role.
    /// </summary>
    public bool HasAdminRole { get; init; }

    /// <summary>
    /// Gets a value indicating whether the current user has permission to create new user accounts.
    /// </summary>
    public bool CanCreateUsers { get; init; }
}

/// <summary>
/// Represents a key-value pair used to provide custom context information for an operation or request.
/// </summary>
/// <remarks>Use this record to attach additional metadata or contextual data to requests where extensibility is
/// required. Both the key and value are user-defined and can be used to convey arbitrary information relevant to the
/// consuming system.</remarks>
public record CustomContextRequest
{
    /// <summary>
    /// Gets the unique identifier associated with this instance.
    /// </summary>
    public string Key { get; init; } = null!;

    /// <summary>
    /// Gets the string value represented by this instance.
    /// </summary>
    public string Value { get; init; } = null!;
}

/// <summary>
/// Represents a response containing contextual information associated with a custom key.
/// </summary>
public record CustomContextResponse
{
    /// <summary>
    /// Gets the unique identifier associated with this instance.
    /// </summary>
    public string Key { get; init; } = null!;

    /// <summary>
    /// Gets the user context from which the operation originated.
    /// </summary>
    public string? FromUserContext { get; init; }

    /// <summary>
    /// Gets the value obtained from the HTTP context, if available.
    /// </summary>
    public string? FromHttpContext { get; init; }
}

/// <summary>
/// Represents the result of a permission check for a specific user and permission.
/// </summary>
/// <remarks>Use this record to convey whether a user has a particular permission, along with the user's
/// identifier and a list of all permissions assigned to the user. This type is typically used as a response in
/// authorization or access control scenarios.</remarks>
public record PermissionCheckResponse
{
    /// <summary>
    /// Gets the name of the permission associated with the current object.
    /// </summary>
    public string Permission { get; init; } = null!;

    /// <summary>
    /// Gets a value indicating whether the current user has the required permission.
    /// </summary>
    public bool HasPermission { get; init; }

    /// <summary>
    /// Gets the unique identifier for the user.
    /// </summary>
    public Guid UserId { get; init; }

    /// <summary>
    /// Gets the complete list of permissions assigned to the current instance.
    /// </summary>
    public List<string> AllPermissions { get; init; } = [];
}

/// <summary>
/// Represents the result of a role membership check for a user, including the user's identifier, the role queried,
/// whether the user has the role, and a list of all roles assigned to the user.
/// </summary>
public record RoleCheckResponse
{
    /// <summary>
    /// Gets the role associated with the current entity or user.
    /// </summary>
    public string Role { get; init; } = null!;

    /// <summary>
    /// Gets a value indicating whether the user is assigned a role.
    /// </summary>
    public bool HasRole { get; init; }

    /// <summary>
    /// Gets the unique identifier for the user.
    /// </summary>
    public Guid UserId { get; init; }

    /// <summary>
    /// Gets the list of all roles available in the system.
    /// </summary>
    public List<string> AllRoles { get; init; } = [];
}
#endregion
