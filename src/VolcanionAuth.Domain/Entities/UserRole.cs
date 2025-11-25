using VolcanionAuth.Domain.Common;

namespace VolcanionAuth.Domain.Entities;

/// <summary>
/// Represents the association between a user and a role within the system.
/// </summary>
/// <remarks>A UserRole instance links a specific user to a specific role, indicating that the user has been
/// assigned the given role. This type is typically used in authorization scenarios to manage user permissions and
/// access control.</remarks>
public class UserRole : Entity<Guid>
{
    /// <summary>
    /// Gets the unique identifier for the user.
    /// </summary>
    public Guid UserId { get; private set; }
    /// <summary>
    /// Gets the unique identifier of the associated role.
    /// </summary>
    public Guid RoleId { get; private set; }
    /// <summary>
    /// Gets the date and time when the assignment was made.
    /// </summary>
    public DateTime AssignedAt { get; private set; }

    #region Navigation
    /// <summary>
    /// Gets the user associated with this entity.
    /// </summary>
    public User User { get; private set; } = null!;
    /// <summary>
    /// Gets the role assigned to the user.
    /// </summary>
    public Role Role { get; private set; } = null!;
    #endregion

    /// <summary>
    /// Initializes a new instance of the UserRole class for use by Entity Framework Core.
    /// </summary>
    /// <remarks>This constructor is intended for use by Entity Framework Core to support object
    /// materialization. It should not be called directly in application code.</remarks>
    private UserRole() { }

    /// <summary>
    /// Initializes a new instance of the UserRole class with the specified user and role identifiers.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to whom the role is assigned.</param>
    /// <param name="roleId">The unique identifier of the role being assigned to the user.</param>
    private UserRole(Guid userId, Guid roleId)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        RoleId = roleId;
        AssignedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a new UserRole instance that associates a user with a role.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to associate with the role.</param>
    /// <param name="roleId">The unique identifier of the role to assign to the user.</param>
    /// <returns>A new UserRole object representing the association between the specified user and role.</returns>
    public static UserRole Create(Guid userId, Guid roleId)
    {
        // Additional validation or business logic can be added here if needed.
        return new UserRole(userId, roleId);
    }
}
