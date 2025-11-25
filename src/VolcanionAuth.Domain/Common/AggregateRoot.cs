namespace VolcanionAuth.Domain.Common;

/// <summary>
/// Base class for aggregate roots
/// </summary>
public abstract class AggregateRoot<TId> : Entity<TId> where TId : notnull
{
    protected AggregateRoot() { }
}
