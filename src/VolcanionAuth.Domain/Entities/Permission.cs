using VolcanionAuth.Domain.Common;

namespace VolcanionAuth.Domain.Entities;

/// <summary>
/// Represents an access permission for a specific resource and action within the system.
/// </summary>
/// <remarks>A permission defines the allowed action (such as 'Read', 'Write', or 'Delete') on a particular
/// resource (such as an entity or feature). Permissions are typically assigned to roles to control access throughout
/// the application. Instances of this class are immutable after creation.</remarks>
public class Permission : AggregateRoot<Guid>
{
    /// <summary>
    /// Gets the identifier or path of the associated resource.
    /// </summary>
    public string Resource { get; private set; } = null!;
    /// <summary>
    /// Gets the name of the action associated with this instance.
    /// </summary>
    public string Action { get; private set; } = null!;
    /// <summary>
    /// Gets the description associated with the current instance.
    /// </summary>
    public string? Description { get; private set; }
    /// <summary>
    /// Gets the date and time when the object was created.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    #region Navigation
    /// <summary>
    /// RolePermissions associated with this Permission.
    /// </summary>
    private readonly List<RolePermission> _rolePermissions = [];
    /// <summary>
    /// Gets the collection of permissions assigned to the role.
    /// </summary>
    public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();
    #endregion

    /// <summary>
    /// Initializes a new instance of the Permission class for use by Entity Framework Core.
    /// </summary>
    /// <remarks>This constructor is intended for use by Entity Framework Core when materializing objects from
    /// a database. It should not be called directly in application code.</remarks>
    private Permission() { }

    /// <summary>
    /// Initializes a new instance of the Permission class with the specified resource, action, and optional
    /// description.
    /// </summary>
    /// <param name="resource">The name of the resource to which the permission applies. Cannot be null or empty.</param>
    /// <param name="action">The action that is permitted on the specified resource. Cannot be null or empty.</param>
    /// <param name="description">An optional description providing additional context for the permission, or null if no description is provided.</param>
    private Permission(string resource, string action, string? description)
    {
        Id = Guid.NewGuid();
        Resource = resource;
        Action = action;
        Description = description;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a new permission for the specified resource and action.
    /// </summary>
    /// <param name="resource">The name of the resource to which the permission applies. Cannot be null, empty, or consist only of white-space
    /// characters.</param>
    /// <param name="action">The action that the permission allows on the specified resource. Cannot be null, empty, or consist only of
    /// white-space characters.</param>
    /// <param name="description">An optional description of the permission. May be null.</param>
    /// <returns>A <see cref="Result{Permission}"/> containing the created permission if the parameters are valid; otherwise, a
    /// failure result with an error message.</returns>
    public static Result<Permission> Create(string resource, string action, string? description = null)
    {
        // Validate input parameters
        if (string.IsNullOrWhiteSpace(resource))
        {
            // Return failure result if resource is null, empty, or whitespace
            return Result.Failure<Permission>("Resource cannot be empty.");
        }
        // Validate action parameter
        if (string.IsNullOrWhiteSpace(action))
        {
            // Return failure result if action is null, empty, or whitespace
            return Result.Failure<Permission>("Action cannot be empty.");
        }
        // Create new Permission instance
        var permission = new Permission(resource, action, description);
        // Return success result with the created permission
        return Result.Success(permission);
    }

    /// <summary>
    /// Returns a string representation of the permission in the format "Resource:Action".
    /// </summary>
    /// <returns>A string that combines the resource and action, separated by a colon.</returns>
    public string GetPermissionString() => $"{Resource}:{Action}";
}
