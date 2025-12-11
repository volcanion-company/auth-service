using VolcanionAuth.Domain.Common;

namespace VolcanionAuth.Domain.Entities;

/// <summary>
/// Represents a relationship between two users, such as a reporting line or team membership.
/// </summary>
/// <remarks>A user relationship defines how one user is connected to another within the system, such as
/// 'manager', 'team_member', or 'owns'. This class is typically used to model organizational hierarchies or user
/// associations. Instances are immutable after creation.</remarks>
public class UserRelationship : Entity<Guid>
{
    /// <summary>
    /// Gets the unique identifier of the user who initiated the operation.
    /// </summary>
    public Guid SourceUserId { get; private set; }
    /// <summary>
    /// Gets the unique identifier of the target user associated with this instance.
    /// </summary>
    public Guid TargetUserId { get; private set; }
    /// <summary>
    /// Gets the type of relationship represented by this instance.
    /// </summary>
    /// <remarks>The relationship type indicates the nature of the connection between the source user.
    /// Value examples include 'manager', 'team_member', or 'owns'. This property is immutable after creation.</remarks>
    public string RelationshipType { get; private set; } = null!;
    /// <summary>
    /// Gets the date and time when the object was created.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    #region Navigation
    /// <summary>
    /// Gets the user who initiated the action or event associated with this entity.
    /// </summary>
    public User SourceUser { get; private set; } = null!;
    /// <summary>
    /// Gets the user that is the target of the current operation.
    /// </summary>
    public User TargetUser { get; private set; } = null!;
    #endregion

    /// <summary>
    /// Initializes a new instance of the UserRelationship class for use by Entity Framework Core.
    /// </summary>
    /// <remarks>This constructor is intended for use by Entity Framework Core when materializing objects from
    /// a database. It should not be called directly in application code.</remarks>
    private UserRelationship() { }

    /// <summary>
    /// Initializes a new instance of the UserRelationship class with the specified source user, target user, and
    /// relationship type.
    /// </summary>
    /// <param name="sourceUserId">The unique identifier of the user who is the source of the relationship.</param>
    /// <param name="targetUserId">The unique identifier of the user who is the target of the relationship.</param>
    /// <param name="relationshipType">The type of relationship between the source and target users. Cannot be null or empty.</param>
    private UserRelationship(Guid sourceUserId, Guid targetUserId, string relationshipType)
    {
        Id = Guid.NewGuid();
        SourceUserId = sourceUserId;
        TargetUserId = targetUserId;
        RelationshipType = relationshipType;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a new user relationship with the specified source user, target user, and relationship type.
    /// </summary>
    /// <param name="sourceUserId">The unique identifier of the user initiating the relationship.</param>
    /// <param name="targetUserId">The unique identifier of the user who is the target of the relationship.</param>
    /// <param name="relationshipType">The type of relationship to create. This value cannot be null or empty.</param>
    /// <returns>A new <see cref="UserRelationship"/> instance representing the relationship between the specified users.</returns>
    public static UserRelationship Create(Guid sourceUserId, Guid targetUserId, string relationshipType)
    {
        // Validation
        return new UserRelationship(sourceUserId, targetUserId, relationshipType);
    }
}
