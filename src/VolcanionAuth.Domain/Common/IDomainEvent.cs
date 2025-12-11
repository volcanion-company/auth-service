using MediatR;

namespace VolcanionAuth.Domain.Common;

/// <summary>
/// Represents a domain event that captures a significant change or occurrence within the domain model.
/// </summary>
/// <remarks>Domain events are used to communicate state changes or business events within a system, often as part
/// of domain-driven design (DDD) patterns. Implementations should provide details about the specific event and the time
/// it occurred. This interface extends <see cref="INotification"/>, allowing domain events to be published and handled
/// using the MediatR library or similar notification mechanisms.</remarks>
public interface IDomainEvent : INotification
{
    /// <summary>
    /// Gets the date and time at which the event occurred.
    /// </summary>
    DateTime OccurredOn { get; }
}
