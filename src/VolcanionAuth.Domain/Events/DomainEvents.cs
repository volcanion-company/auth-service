using VolcanionAuth.Domain.Common;
using VolcanionAuth.Domain.ValueObjects;

namespace VolcanionAuth.Domain.Events;

/// <summary>
/// Represents a domain event that occurs when a new user has been registered.
/// </summary>
/// <param name="UserId">The unique identifier of the user who has been registered.</param>
/// <param name="Email">The email address associated with the newly registered user.</param>
public record UserRegisteredEvent(Guid UserId, Email Email) : IDomainEvent
{
    /// <summary>
    /// Gets the date and time when the event occurred, in Coordinated Universal Time (UTC).
    /// </summary>
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

/// <summary>
/// Represents a domain event that occurs when a user successfully logs in.
/// </summary>
/// <param name="UserId">The unique identifier of the user who has logged in.</param>
/// <param name="IpAddress">The IP address from which the user logged in.</param>
public record UserLoggedInEvent(Guid UserId, string IpAddress) : IDomainEvent
{
    /// <summary>
    /// Gets the date and time at which the event occurred, in Coordinated Universal Time (UTC).
    /// </summary>
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

/// <summary>
/// Represents a domain event that occurs when a user's password is changed.
/// </summary>
/// <param name="UserId">The unique identifier of the user whose password was changed.</param>
public record UserPasswordChangedEvent(Guid UserId) : IDomainEvent
{
    /// <summary>
    /// Gets the date and time when the event occurred, in Coordinated Universal Time (UTC).
    /// </summary>
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

/// <summary>
/// Represents a domain event that occurs when a user's email address has been successfully verified.
/// </summary>
/// <param name="UserId">The unique identifier of the user whose email address was verified.</param>
public record UserEmailVerifiedEvent(Guid UserId) : IDomainEvent
{
    /// <summary>
    /// Gets the date and time when the event occurred, in Coordinated Universal Time (UTC).
    /// </summary>
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

/// <summary>
/// Represents a domain event that occurs when a user account is locked until a specified date and time.
/// </summary>
/// <param name="UserId">The unique identifier of the user whose account has been locked.</param>
/// <param name="LockedUntil">The date and time, in UTC, until which the user account remains locked.</param>
public record UserLockedEvent(Guid UserId, DateTime LockedUntil) : IDomainEvent
{
    /// <summary>
    /// Gets the date and time when the event occurred, in Coordinated Universal Time (UTC).
    /// </summary>
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

/// <summary>
/// Represents a domain event that occurs when a user account is deactivated.
/// </summary>
/// <param name="UserId">The unique identifier of the user whose account has been deactivated.</param>
public record UserDeactivatedEvent(Guid UserId) : IDomainEvent
{
    /// <summary>
    /// Gets the date and time when the event occurred, in Coordinated Universal Time (UTC).
    /// </summary>
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

/// <summary>
/// Represents a domain event that occurs when a role is assigned to a user.
/// </summary>
/// <param name="UserId">The unique identifier of the user to whom the role is assigned.</param>
/// <param name="RoleId">The unique identifier of the role that is assigned to the user.</param>
public record UserRoleAssignedEvent(Guid UserId, Guid RoleId) : IDomainEvent
{
    /// <summary>
    /// Gets the date and time when the event occurred, in Coordinated Universal Time (UTC).
    /// </summary>
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

/// <summary>
/// Represents a domain event that occurs when a role is removed from a user.
/// </summary>
/// <param name="UserId">The unique identifier of the user from whom the role was removed.</param>
/// <param name="RoleId">The unique identifier of the role that was removed from the user.</param>
public record UserRoleRemovedEvent(Guid UserId, Guid RoleId) : IDomainEvent
{
    /// <summary>
    /// Gets the date and time at which the event occurred, in Coordinated Universal Time (UTC).
    /// </summary>
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

/// <summary>
/// Represents a domain event that occurs when a new role is created.
/// </summary>
/// <param name="RoleId">The unique identifier of the role that was created.</param>
/// <param name="RoleName">The name of the role that was created.</param>
public record RoleCreatedEvent(Guid RoleId, string RoleName) : IDomainEvent
{
    /// <summary>
    /// Gets the date and time when the event occurred, in Coordinated Universal Time (UTC).
    /// </summary>
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

/// <summary>
/// Represents an event that occurs when a new policy is created.
/// </summary>
/// <param name="PolicyId">The unique identifier of the created policy.</param>
/// <param name="PolicyName">The name of the created policy.</param>
public record PolicyCreatedEvent(Guid PolicyId, string PolicyName) : IDomainEvent
{
    /// <summary>
    /// Gets the date and time at which the event occurred, in Coordinated Universal Time (UTC).
    /// </summary>
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
