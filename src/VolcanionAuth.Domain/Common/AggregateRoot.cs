namespace VolcanionAuth.Domain.Common;

/// <summary>
/// Represents the base class for aggregate roots in a domain-driven design context, providing a unique identity and
/// encapsulating domain logic for a group of related entities.
/// </summary>
/// <remarks>Aggregate roots serve as the entry point for modifying and retrieving related entities within an
/// aggregate. All changes to the aggregate should be performed through the aggregate root to maintain consistency and
/// enforce invariants.</remarks>
/// <typeparam name="TId">The type of the unique identifier for the aggregate root. Must be a non-nullable type.</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId> where TId : notnull
{
    /// <summary>
    /// Initializes a new instance of the AggregateRoot class.
    /// </summary>
    /// <remarks>This constructor is intended to be called by derived classes to ensure proper initialization
    /// of the aggregate root. It is typically used in domain-driven design patterns to represent the root entity of an
    /// aggregate.</remarks>
    protected AggregateRoot() { }
}
