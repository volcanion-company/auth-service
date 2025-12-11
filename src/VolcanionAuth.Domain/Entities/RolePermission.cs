using VolcanionAuth.Domain.Common;

namespace VolcanionAuth.Domain.Entities;

/// <summary>
/// Represents the association between a role and a permission within the system.
/// </summary>
/// <remarks>A RolePermission instance links a specific role to a specific permission, indicating that the role
/// has been granted the permission. This type is typically used to manage and query role-based access control
/// assignments.</remarks>
public class RolePermission : Entity<Guid>
{
    /// <summary>
    /// Gets the unique identifier of the associated role.
    /// </summary>
    public Guid RoleId { get; private set; }
    /// <summary>
    /// Gets the unique identifier for the associated permission.
    /// </summary>
    public Guid PermissionId { get; private set; }
    /// <summary>
    /// Gets the date and time when the assignment was made.
    /// </summary>
    public DateTime AssignedAt { get; private set; }

    #region Navigation
    /// <summary>
    /// Gets the role associated with this entity.
    /// </summary>
    public Role Role { get; private set; } = null!;
    /// <summary>
    /// Gets the permission associated with the current context.
    /// </summary>
    public Permission Permission { get; private set; } = null!;
    #endregion

    /// <summary>
    /// Initializes a new instance of the RolePermission class for use by Entity Framework Core.
    /// </summary>
    /// <remarks>This constructor is intended for use by Entity Framework Core when materializing objects from
    /// a database. It should not be called directly in application code.</remarks>
    private RolePermission() { }

    /// <summary>
    /// Initializes a new instance of the RolePermission class with the specified role and permission identifiers.
    /// </summary>
    /// <param name="roleId">The unique identifier of the role to which the permission is assigned.</param>
    /// <param name="permissionId">The unique identifier of the permission to assign to the role.</param>
    private RolePermission(Guid roleId, Guid permissionId)
    {
        Id = Guid.NewGuid();
        RoleId = roleId;
        PermissionId = permissionId;
        AssignedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a new instance of the RolePermission class that associates a role with a permission.
    /// </summary>
    /// <param name="roleId">The unique identifier of the role to associate with the permission.</param>
    /// <param name="permissionId">The unique identifier of the permission to assign to the role.</param>
    /// <returns>A new RolePermission instance representing the association between the specified role and permission.</returns>
    public static RolePermission Create(Guid roleId, Guid permissionId)
    {
        // Additional validation or business logic can be added here if needed.
        return new RolePermission(roleId, permissionId);
    }
}
