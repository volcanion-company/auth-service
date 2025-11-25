using VolcanionAuth.Domain.Common;
using VolcanionAuth.Domain.Events;

namespace VolcanionAuth.Domain.Entities;

/// <summary>
/// Represents a role within the system, including its name, description, status, and associated permissions and users.
/// </summary>
/// <remarks>A role defines a set of permissions that can be assigned to users for access control purposes. Roles
/// can be activated or deactivated, and their details can be updated after creation. The class maintains collections of
/// associated permissions and users, which are exposed as read-only collections. Role instances are created and managed
/// through static and instance methods that enforce validation and domain rules.</remarks>
public class Role : AggregateRoot<Guid>
{
    /// <summary>
    /// Gets the name associated with the current instance.
    /// </summary>
    public string Name { get; private set; } = null!;
    /// <summary>
    /// Gets the description associated with the current instance.
    /// </summary>
    public string? Description { get; private set; }
    /// <summary>
    /// Gets a value indicating whether the object is currently active.
    /// </summary>
    public bool IsActive { get; private set; }
    /// <summary>
    /// Gets the date and time when the object was created.
    /// </summary>
    public DateTime CreatedAt { get; private set; }
    /// <summary>
    /// Gets the date and time when the entity was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; private set; }

    #region Navigation properties
    /// <summary>
    /// Role permissions associated with the current instance.
    /// </summary>
    private readonly List<RolePermission> _rolePermissions = [];
    /// <summary>
    /// Gets the collection of permissions associated with the role.
    /// </summary>
    public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();
    /// <summary>
    /// User roles associated with the current instance.
    /// </summary>
    private readonly List<UserRole> _userRoles = [];
    /// <summary>
    /// Gets the collection of roles assigned to the user.
    /// </summary>
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();
    #endregion

    /// <summary>
    /// Initializes a new instance of the Role class for use by Entity Framework Core.
    /// </summary>
    /// <remarks>This constructor is intended for use by Entity Framework Core to materialize Role instances
    /// from the database. It should not be called directly in application code.</remarks>
    private Role() { }

    /// <summary>
    /// Initializes a new instance of the Role class with the specified name and optional description.
    /// </summary>
    /// <param name="name">The unique name that identifies the role. Cannot be null or empty.</param>
    /// <param name="description">An optional description providing additional details about the role, or null if no description is provided.</param>
    private Role(string name, string? description)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a new role with the specified name and optional description.
    /// </summary>
    /// <param name="name">The name of the role to create. Cannot be null, empty, or consist only of white-space characters. The maximum
    /// length is 100 characters.</param>
    /// <param name="description">An optional description for the role. May be null.</param>
    /// <returns>A result containing the newly created role if the operation succeeds; otherwise, a failure result with an error
    /// message.</returns>
    public static Result<Role> Create(string name, string? description = null)
    {
        // Validate role name
        if (string.IsNullOrWhiteSpace(name))
        {
            // Return failure result if name is null, empty, or whitespace
            return Result.Failure<Role>("Role name cannot be empty.");
        }
        // Validate role name length
        if (name.Length > 100)
        {
            // Return failure result if name exceeds maximum length
            return Result.Failure<Role>("Role name cannot exceed 100 characters.");
        }
        // Create new role instance
        var role = new Role(name, description);
        // Add domain event for role creation
        role.AddDomainEvent(new RoleCreatedEvent(role.Id, role.Name));
        // Return success result with the created role
        return Result.Success(role);
    }

    /// <summary>
    /// Updates the role's name and description.
    /// </summary>
    /// <param name="name">The new name for the role. Cannot be null, empty, or consist only of white-space characters.</param>
    /// <param name="description">The new description for the role, or null to remove the existing description.</param>
    /// <returns>A <see cref="Result"/> indicating whether the update operation succeeded. Returns a failure result if <paramref
    /// name="name"/> is null, empty, or white space.</returns>
    public Result Update(string name, string? description)
    {
        // Validate role name
        if (string.IsNullOrWhiteSpace(name))
        {
            // Return failure result if name is null, empty, or whitespace
            return Result.Failure("Role name cannot be empty.");
        }

        // Update role properties
        Name = name;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
        // Return success result
        return Result.Success();
    }

    /// <summary>
    /// Adds a permission to the role if it is not already assigned.
    /// </summary>
    /// <remarks>If the specified permission is already assigned to the role, this method has no effect. The
    /// role's last updated timestamp is modified when a new permission is added.</remarks>
    /// <param name="permissionId">The unique identifier of the permission to add to the role.</param>
    public void AddPermission(Guid permissionId)
    {
        // Check if the permission is already assigned to the role
        if (_rolePermissions.Any(rp => rp.PermissionId == permissionId))
        {
            // Permission already assigned; no action needed
            return;
        }

        // Create and add the new role-permission association
        var rolePermission = RolePermission.Create(Id, permissionId);
        // Assuming RolePermission.Create always succeeds; otherwise, handle the Result accordingly
        _rolePermissions.Add(rolePermission);
        // Update the last updated timestamp
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Removes the permission with the specified identifier from the role, if it exists.
    /// </summary>
    /// <param name="permissionId">The unique identifier of the permission to remove.</param>
    public void RemovePermission(Guid permissionId)
    {
        // Find the role-permission association to remove
        var rolePermission = _rolePermissions.FirstOrDefault(rp => rp.PermissionId == permissionId);
        if (rolePermission != null)
        {
            // Remove the association from the collection
            _rolePermissions.Remove(rolePermission);
            // Update the last updated timestamp
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Deactivates the current instance and updates its last modified timestamp.
    /// </summary>
    /// <returns>A <see cref="Result"/> indicating whether the deactivation was successful.</returns>
    public Result Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    /// <summary>
    /// Activates the current instance and updates its last modified timestamp.
    /// </summary>
    /// <returns>A <see cref="Result"/> indicating whether the activation was successful.</returns>
    public Result Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }
}
