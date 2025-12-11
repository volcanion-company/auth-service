using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.API.Services;

/// <summary>
/// Provides access to the current user's context, including identity, roles, permissions, and custom data, for use
/// within the application.
/// </summary>
/// <remarks>UserContextService centralizes user-related data for the current request, making it available to
/// other components. It is typically used in scenarios where user identity, roles, or permissions are required for
/// authorization or personalization. The service is not thread-safe and should be scoped per request.</remarks>
/// <param name="authorizationService">The service used to retrieve user permissions and perform authorization checks.</param>
/// <param name="httpContextAccessor">The accessor for obtaining the current HTTP context, which may contain user-related information.</param>
/// <param name="readRepository">The repository for reading user data from the database.</param>
/// <param name="logger">The logger used to record diagnostic and error information related to user context operations.</param>
public class UserContextService(
    IAuthorizationService authorizationService,
    IHttpContextAccessor httpContextAccessor,
    IReadRepository<User> readRepository,
    ILogger<UserContextService> logger) : IUserContextService
{
    /// <summary>
    /// Gets the unique identifier of the user associated with the current context, if available.
    /// </summary>
    public Guid? UserId { get; private set; }

    /// <summary>
    /// Gets the email address associated with the user.
    /// </summary>
    public string? UserEmail { get; private set; }

    /// <summary>
    /// Gets the first name of the user.
    /// </summary>
    public string? FirstName { get; private set; }

    /// <summary>
    /// Gets the last name of the user.
    /// </summary>
    public string? LastName { get; private set; }

    /// <summary>
    /// Gets the list of roles assigned to the user.
    /// </summary>
    /// <remarks>The collection is read-only and reflects the current roles associated with the user.
    /// Modifications to the list should be performed through designated methods or APIs that manage user
    /// roles.</remarks>
    public List<string> Roles { get; private set; } = [];

    /// <summary>
    /// Gets the list of permission identifiers assigned to the current instance.
    /// </summary>
    public List<string> Permissions { get; private set; } = [];

    /// <summary>
    /// Gets the collection of custom key-value pairs associated with this instance.
    /// </summary>
    /// <remarks>Use this property to store additional metadata or user-defined information that is not
    /// covered by other properties. The dictionary is read-only; to modify its contents, use its methods to add,
    /// update, or remove entries.</remarks>
    public Dictionary<string, object> CustomData { get; private set; } = [];

    /// <inheritdoc />
    public async Task LoadUserContextAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Set the user ID
        UserId = userId;
        
        // Load user from database to get full information
        try
        {
            var user = await readRepository.GetByIdAsync(userId, cancellationToken);
            if (user != null)
            {
                UserEmail = user.Email.Value;
                FirstName = user.FullName.FirstName;
                LastName = user.FullName.LastName;
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to load user data for user {UserId}", userId);
        }
        
        // Load user email and roles from HTTP context as fallback
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            // Load email if not already loaded
            UserEmail ??= httpContext.Items["UserEmail"]?.ToString();
            // Load roles
            if (httpContext.Items["UserRoles"] is List<string> roles)
            {
                Roles = roles;
            }
        }

        // Load user permissions
        try
        {
            // Retrieve permissions from the authorization service
            var permissions = await authorizationService.GetUserPermissionsAsync(userId, cancellationToken);
            Permissions = [.. permissions];
            // Log the number of loaded permissions
            logger.LogDebug(
                "Loaded {PermissionCount} permissions for user {UserId}",
                Permissions.Count,
                userId);
        }
        catch (Exception ex)
        {
            // Log any exceptions that occur during permission loading
            logger.LogWarning(ex, "Failed to load permissions for user {UserId}", userId);
        }
    }

    /// <inheritdoc />
    public void SetCustomData(string key, object value)
    {
        // Set or update the custom data entry
        CustomData[key] = value;
    }

    /// <inheritdoc />
    public T? GetCustomData<T>(string key)
    {
        // Attempt to retrieve and cast the custom data entry
        if (CustomData.TryGetValue(key, out var value) && value is T typedValue)
        {
            // Return the cast value if successful
            return typedValue;
        }
        // Return default value if key not found or cast fails
        return default;
    }

    /// <inheritdoc />
    public bool HasPermission(string permission)
    {
        // Check if the permission exists in the list using case-insensitive comparison
        return Permissions.Contains(permission, StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public bool HasRole(string role)
    {
        // Check if the role exists in the list using case-insensitive comparison
        return Roles.Contains(role, StringComparer.OrdinalIgnoreCase);
    }
}
