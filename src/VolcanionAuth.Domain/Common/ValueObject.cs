namespace VolcanionAuth.Domain.Common;

/// <summary>
/// Provides a base class for value objects, enabling value-based equality comparison and hash code generation according
/// to the components defined by derived types.
/// </summary>
/// <remarks>Inherit from this class to implement value objects that compare equality based on their constituent
/// values rather than object identity. Derived classes must override the GetEqualityComponents method to specify which
/// properties participate in equality and hash code calculations. Two value objects are considered equal if all their
/// equality components are equal in order. This class also provides equality (==) and inequality (!=) operators for
/// convenience.</remarks>
public abstract class ValueObject
{
    /// <summary>
    /// When implemented in a derived class, returns the components that are used to determine equality for the current
    /// object.
    /// </summary>
    /// <remarks>Override this method in a derived class to specify which fields or properties should
    /// participate in equality comparisons. All components returned should be immutable or treated as such to ensure
    /// consistent behavior.</remarks>
    /// <returns>An enumerable collection of objects that represent the values to be used for equality comparisons. The order of
    /// the components is significant.</returns>
    protected abstract IEnumerable<object?> GetEqualityComponents();

    /// <summary>
    /// Determines whether the specified object is equal to the current value object.
    /// </summary>
    /// <remarks>Two value objects are considered equal if they are of the same type and all their equality
    /// components are equal. This method is intended to be used in value object equality comparisons.</remarks>
    /// <param name="obj">The object to compare with the current value object.</param>
    /// <returns>true if the specified object is equal to the current value object; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        // Check for null and compare run-time types.
        if (obj == null || obj.GetType() != GetType())
        {
            // obj is null or types do not match
            return false;
        }
        // Compare each component for equality
        var other = (ValueObject)obj;
        // SequenceEqual handles the case of different lengths
        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    /// <summary>
    /// Serves as the default hash function for the object, providing a hash code based on the object's equality
    /// components.
    /// </summary>
    /// <remarks>Use this method when storing objects in hash-based collections such as Dictionary or HashSet.
    /// The hash code is computed from the components that determine object equality, ensuring that equal objects have
    /// the same hash code.</remarks>
    /// <returns>A 32-bit signed integer hash code that represents the current object.</returns>
    public override int GetHashCode()
    {
        // Combine the hash codes of all equality components
        return GetEqualityComponents().Select(x => x?.GetHashCode() ?? 0).Aggregate((x, y) => x ^ y);
    }

    /// <summary>
    /// Determines whether two specified ValueObject instances are equal.
    /// </summary>
    /// <remarks>Equality is determined by the ValueObject's implementation of the Equals method. This
    /// operator provides value-based equality comparison rather than reference equality.</remarks>
    /// <param name="left">The first ValueObject to compare, or null.</param>
    /// <param name="right">The second ValueObject to compare, or null.</param>
    /// <returns>true if the two ValueObject instances are equal or both are null; otherwise, false.</returns>
    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        // Check for null on both sides
        if (left is null && right is null)
        {
            // Both are null
            return true;
        }
        // One is null, the other is not
        if (left is null || right is null)
        {
            // One is null
            return false;
        }
        // Both are not null, use Equals method
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two specified ValueObject instances are not equal.
    /// </summary>
    /// <param name="left">The first ValueObject to compare, or null.</param>
    /// <param name="right">The second ValueObject to compare, or null.</param>
    /// <returns>true if the specified ValueObject instances are not equal; otherwise, false.</returns>
    public static bool operator !=(ValueObject? left, ValueObject? right)
    {
        // Use the equality operator to determine inequality
        return !(left == right);
    }
}
