using VolcanionAuth.Domain.Common;

namespace VolcanionAuth.Domain.Entities;

/// <summary>
/// Represents a custom attribute associated with a user, including its key, value, and data type.
/// </summary>
/// <remarks>User attributes allow for the storage of additional metadata or profile information for a user beyond
/// the standard user properties. Each attribute is uniquely identified by its key and is linked to a specific user. The
/// data type indicates how the attribute value should be interpreted (for example, as a string, number, boolean, or
/// date).</remarks>
public class UserAttribute : Entity<Guid>
{
    /// <summary>
    /// Gets the unique identifier for the user.
    /// </summary>
    public Guid UserId { get; private set; }
    /// <summary>
    /// Gets the key that identifies the attribute.
    /// </summary>
    public string AttributeKey { get; private set; } = null!;
    /// <summary>
    /// Gets the value associated with the attribute.
    /// </summary>
    public string AttributeValue { get; private set; } = null!;
    /// <summary>
    /// Gets the data type represented by the property.
    /// </summary>
    /// <remarks>Common values include "string", "number", "boolean", and "date". The set of supported data
    /// types may vary depending on the context in which this property is used.</remarks>
    public string DataType { get; private set; } = null!;
    /// <summary>
    /// Gets the date and time when the object was created.
    /// </summary>
    public DateTime CreatedAt { get; private set; }
    /// <summary>
    /// Gets the date and time when the entity was last updated, or null if the entity has not been updated.
    /// </summary>
    public DateTime? UpdatedAt { get; private set; }

    #region Navigation
    /// <summary>
    /// Gets the user associated with this entity.
    /// </summary>
    public User User { get; private set; } = null!;
    #endregion

    /// <summary>
    /// Initializes a new instance of the UserAttribute class for use by Entity Framework Core.
    /// </summary>
    /// <remarks>This constructor is intended for use by Entity Framework Core and should not be called
    /// directly in application code.</remarks>
    private UserAttribute() { }

    /// <summary>
    /// Initializes a new instance of the UserAttribute class with the specified user identifier, attribute key, value,
    /// and data type.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to whom the attribute belongs.</param>
    /// <param name="key">The key that identifies the attribute. Cannot be null or empty.</param>
    /// <param name="value">The value associated with the attribute key. Cannot be null.</param>
    /// <param name="dataType">The data type of the attribute value, such as "string", "int", or "date". Cannot be null or empty.</param>
    private UserAttribute(Guid userId, string key, string value, string dataType)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        AttributeKey = key;
        AttributeValue = value;
        DataType = dataType;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a new instance of the UserAttribute class with the specified user identifier, key, value, and data type.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to whom the attribute belongs.</param>
    /// <param name="key">The name of the attribute to create. Cannot be null or empty.</param>
    /// <param name="value">The value to assign to the attribute. Cannot be null.</param>
    /// <param name="dataType">The data type of the attribute value. Defaults to "string" if not specified.</param>
    /// <returns>A new UserAttribute instance initialized with the provided user identifier, key, value, and data type.</returns>
    public static UserAttribute Create(Guid userId, string key, string value, string dataType = "string")
    {
        // Input validation
        return new UserAttribute(userId, key, value, dataType);
    }

    /// <summary>
    /// Updates the attribute value and records the current time as the last update timestamp.
    /// </summary>
    /// <remarks>This method sets the attribute value and updates the timestamp to the current UTC time. Use
    /// this method to ensure that the last updated time reflects the most recent change.</remarks>
    /// <param name="value">The new value to assign to the attribute. Can be null or empty.</param>
    public void UpdateValue(string newValue)
    {
        AttributeValue = newValue;
        UpdatedAt = DateTime.UtcNow;
    }
}
