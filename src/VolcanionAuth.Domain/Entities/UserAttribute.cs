using VolcanionAuth.Domain.Common;

namespace VolcanionAuth.Domain.Entities;

/// <summary>
/// User attribute for ABAC dynamic attributes
/// </summary>
public class UserAttribute : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public string AttributeKey { get; private set; } = null!;
    public string AttributeValue { get; private set; } = null!;
    public string DataType { get; private set; } = null!; // string, number, boolean, date
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Navigation
    public User User { get; private set; } = null!;

    private UserAttribute() { } // EF Core

    private UserAttribute(Guid userId, string key, string value, string dataType)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        AttributeKey = key;
        AttributeValue = value;
        DataType = dataType;
        CreatedAt = DateTime.UtcNow;
    }

    public static UserAttribute Create(Guid userId, string key, string value, string dataType = "string")
    {
        return new UserAttribute(userId, key, value, dataType);
    }

    public void UpdateValue(string value)
    {
        AttributeValue = value;
        UpdatedAt = DateTime.UtcNow;
    }
}
