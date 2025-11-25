using VolcanionAuth.Domain.Common;

namespace VolcanionAuth.Domain.Entities;

/// <summary>
/// User relationship for ReBAC (Relationship-Based Access Control)
/// </summary>
public class UserRelationship : Entity<Guid>
{
    public Guid SourceUserId { get; private set; }
    public Guid TargetUserId { get; private set; }
    public string RelationshipType { get; private set; } = null!; // e.g., "manager", "team_member", "owns"
    public DateTime CreatedAt { get; private set; }

    // Navigation
    public User SourceUser { get; private set; } = null!;
    public User TargetUser { get; private set; } = null!;

    private UserRelationship() { } // EF Core

    private UserRelationship(Guid sourceUserId, Guid targetUserId, string relationshipType)
    {
        Id = Guid.NewGuid();
        SourceUserId = sourceUserId;
        TargetUserId = targetUserId;
        RelationshipType = relationshipType;
        CreatedAt = DateTime.UtcNow;
    }

    public static UserRelationship Create(Guid sourceUserId, Guid targetUserId, string relationshipType)
    {
        return new UserRelationship(sourceUserId, targetUserId, relationshipType);
    }
}
