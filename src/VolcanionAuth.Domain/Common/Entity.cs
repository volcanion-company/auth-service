namespace VolcanionAuth.Domain.Common;

/// <summary>
/// Represents a base class for entities with a unique identifier and domain event support.
/// </summary>
/// <remarks>This abstract class provides common functionality for domain entities, including identity management
/// and domain event tracking. Entities are considered equal if they are of the same type and have the same identifier.
/// Domain events can be added, removed, or cleared to support domain-driven design patterns.</remarks>
/// <typeparam name="TId">The type of the entity's unique identifier. Must be a non-nullable type.</typeparam>
public abstract class Entity<TId> where TId : notnull
{
    /// <summary>
    /// Gets the unique identifier for the entity.
    /// </summary>
    public TId Id { get; protected set; } = default!;

    /// <summary>
    /// Domain events associated with the entity.
    /// </summary>
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// Gets the collection of domain events that have been raised by the entity.
    /// </summary>
    /// <remarks>The collection is read-only and reflects the current set of domain events associated with the
    /// entity. Domain events are typically used to signal significant changes or actions within the domain model, and
    /// may be cleared or dispatched by the application after processing.</remarks>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Adds a domain event to the collection of events associated with this entity.
    /// </summary>
    /// <remarks>Domain events are used to capture and propagate significant changes or actions within the
    /// domain model. After adding a domain event, it can be processed by event handlers or dispatched as part of the
    /// application's workflow.</remarks>
    /// <param name="domainEvent">The domain event to add. Cannot be null.</param>
    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        // Adds the specified domain event to the list of domain events.
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Removes the specified domain event from the collection of pending domain events.
    /// </summary>
    /// <param name="domainEvent">The domain event to remove from the collection. Cannot be null.</param>
    public void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        // Removes the specified domain event from the list of domain events.
        _domainEvents.Remove(domainEvent);
    }

    /// <summary>
    /// Removes all domain events from the current entity.
    /// </summary>
    /// <remarks>Call this method to clear the list of domain events after they have been processed or
    /// dispatched. This is typically used to prevent duplicate handling of events.</remarks>
    public void ClearDomainEvents()
    {
        // Clears all domain events from the list.
        _domainEvents.Clear();
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current entity based on type and identifier.
    /// </summary>
    /// <remarks>Two entities are considered equal if they are of the same type and their identifiers are
    /// equal. Reference equality is checked before comparing identifiers.</remarks>
    /// <param name="obj">The object to compare with the current entity. Can be null.</param>
    /// <returns>true if the specified object is an entity of the same type and has the same identifier as the current entity;
    /// otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        // Check if the other object is an Entity of the same type
        if (obj is not Entity<TId> other)
        {
            // Not the same type
            return false;
        }
        // Check for reference equality
        if (ReferenceEquals(this, other))
        {
            // Same reference
            return true;
        }
        // Check for type equality
        if (GetType() != other.GetType())
        {
            // Different types
            return false;
        }
        // Check for identifier equality
        return Id.Equals(other.Id);
    }

    /// <summary>
    /// Serves as the default hash function for the object.
    /// </summary>
    /// <remarks>The hash code is based on the object's runtime type and its Id property. This implementation
    /// is suitable for use in hash-based collections such as dictionaries and hash sets.</remarks>
    /// <returns>A 32-bit signed integer hash code that represents the current object.</returns>
    public override int GetHashCode()
    {
        // Combine the type's string representation with the Id to generate a hash code
        return (GetType().ToString() + Id).GetHashCode();
    }

    /// <summary>
    /// Determines whether two Entity<TId> instances are equal.
    /// </summary>
    /// <remarks>Equality is determined by the Equals method. Two null references are considered
    /// equal.</remarks>
    /// <param name="a">The first Entity<TId> instance to compare, or null.</param>
    /// <param name="b">The second Entity<TId> instance to compare, or null.</param>
    /// <returns>true if both instances are null or if they are equal; otherwise, false.</returns>
    public static bool operator ==(Entity<TId>? a, Entity<TId>? b)
    {
        // Check for null references
        if (a is null && b is null)
        {
            // Both are null
            return true;
        }
        // One is null, the other is not
        if (a is null || b is null)
        {
            // One is null
            return false;
        }
        // Both are not null, use Equals method
        return a.Equals(b);
    }

    /// <summary>
    /// Determines whether two specified Entity<TId> instances are not equal.
    /// </summary>
    /// <param name="a">The first Entity<TId> instance to compare, or null.</param>
    /// <param name="b">The second Entity<TId> instance to compare, or null.</param>
    /// <returns>true if the specified Entity<TId> instances are not equal; otherwise, false.</returns>
    public static bool operator !=(Entity<TId>? a, Entity<TId>? b)
    {
        // Use the equality operator to determine inequality
        return !(a == b);
    }
}
