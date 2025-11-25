using VolcanionAuth.Domain.Common;

namespace VolcanionAuth.Domain.Entities;

/// <summary>
/// Login history entity for audit trail
/// </summary>
public class LoginHistory : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public string IpAddress { get; private set; } = null!;
    public string UserAgent { get; private set; } = null!;
    public bool IsSuccessful { get; private set; }
    public DateTime Timestamp { get; private set; }
    public string? FailureReason { get; private set; }

    // Navigation
    public User User { get; private set; } = null!;

    private LoginHistory() { } // EF Core

    private LoginHistory(Guid userId, string ipAddress, string userAgent, bool isSuccessful, string? failureReason = null)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        IsSuccessful = isSuccessful;
        FailureReason = failureReason;
        Timestamp = DateTime.UtcNow;
    }

    public static LoginHistory Create(Guid userId, string ipAddress, string userAgent, bool isSuccessful, string? failureReason = null)
    {
        return new LoginHistory(userId, ipAddress, userAgent, isSuccessful, failureReason);
    }
}
