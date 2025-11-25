using VolcanionAuth.Domain.Common;
using VolcanionAuth.Domain.ValueObjects;

namespace VolcanionAuth.Domain.Events;

public record UserRegisteredEvent(Guid UserId, Email Email) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record UserLoggedInEvent(Guid UserId, string IpAddress) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record UserPasswordChangedEvent(Guid UserId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record UserEmailVerifiedEvent(Guid UserId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record UserLockedEvent(Guid UserId, DateTime LockedUntil) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record UserDeactivatedEvent(Guid UserId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record UserRoleAssignedEvent(Guid UserId, Guid RoleId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record UserRoleRemovedEvent(Guid UserId, Guid RoleId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record RoleCreatedEvent(Guid RoleId, string RoleName) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record PolicyCreatedEvent(Guid PolicyId, string PolicyName) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
