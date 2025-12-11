using VolcanionAuth.Domain.Common;

namespace VolcanionAuth.Domain.ValueObjects;

/// <summary>
/// Represents a person's full name as an immutable value object, consisting of a first name and a last name.
/// </summary>
/// <remarks>The FullName class enforces validation rules to ensure that both the first and last names are
/// non-empty and do not exceed 50 characters. Instances can only be created using the static Create method, which
/// returns a result indicating success or failure. This type is suitable for use in domain models where value-based
/// equality and immutability are required.</remarks>
public sealed class FullName : ValueObject
{
    /// <summary>
    /// Gets the first name of the person.
    /// </summary>
    public string FirstName { get; }
    /// <summary>
    /// Gets the last name of the person.
    /// </summary>
    public string LastName { get; }

    /// <summary>
    /// Initializes a new instance of the FullName class with the specified first and last names.
    /// </summary>
    /// <param name="firstName">The first name component of the full name. Cannot be null.</param>
    /// <param name="lastName">The last name component of the full name. Cannot be null.</param>
    private FullName(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    /// <summary>
    /// Creates a new instance of the FullName class using the specified first and last names, validating that both are
    /// non-empty and do not exceed 50 characters.
    /// </summary>
    /// <param name="firstName">The first name to include in the full name. Cannot be null, empty, or consist only of white-space characters.
    /// Must not exceed 50 characters.</param>
    /// <param name="lastName">The last name to include in the full name. Cannot be null, empty, or consist only of white-space characters.
    /// Must not exceed 50 characters.</param>
    /// <returns>A Result containing the created FullName if the input values are valid; otherwise, a failure Result with an
    /// error message describing the validation issue.</returns>
    public static Result<FullName> Create(string firstName, string lastName)
    {
        // Validate first name
        if (string.IsNullOrWhiteSpace(firstName))
        {
            // Return failure result if first name is null, empty, or whitespace
            return Result.Failure<FullName>("First name cannot be empty.");
        }
        // Validate last name
        if (string.IsNullOrWhiteSpace(lastName))
        {
            // Return failure result if last name is null, empty, or whitespace
            return Result.Failure<FullName>("Last name cannot be empty.");
        }
        // Validate length constraints
        if (firstName.Length > 50)
        {
            // Return failure result if first name exceeds 50 characters
            return Result.Failure<FullName>("First name cannot exceed 50 characters.");
        }
        // Validate length constraints
        if (lastName.Length > 50)
        {
            // Return failure result if last name exceeds 50 characters
            return Result.Failure<FullName>("Last name cannot exceed 50 characters.");
        }
        // Return success result with new FullName instance
        return Result.Success(new FullName(firstName.Trim(), lastName.Trim()));
    }

    /// <summary>
    /// Returns the full name by concatenating the first and last name with a space.
    /// </summary>
    /// <returns>A string containing the full name in the format "FirstName LastName". If either the first or last name is null
    /// or empty, the result may contain extra spaces.</returns>
    public string GetFullName() => $"{FirstName} {LastName}";

    /// <summary>
    /// Provides the components that are used to determine value equality for the current object.
    /// </summary>
    /// <remarks>Override this method to specify which properties or fields should participate in value
    /// equality for derived types. The order of the returned components affects the equality comparison.</remarks>
    /// <returns>An enumerable collection of objects representing the values to be used for equality comparisons.</returns>
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        // Return the first and last name as components for equality comparison
        yield return FirstName;
        yield return LastName;
    }

    /// <summary>
    /// Returns a string that represents the full name of the object.
    /// </summary>
    /// <returns>A string containing the full name of the object.</returns>
    public override string ToString() => GetFullName();
}
