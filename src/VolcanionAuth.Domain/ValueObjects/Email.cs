using System.Text.RegularExpressions;
using VolcanionAuth.Domain.Common;

namespace VolcanionAuth.Domain.ValueObjects;

/// <summary>
/// Represents an immutable, validated email address value object.
/// </summary>
/// <remarks>The Email type ensures that the encapsulated email address is valid according to a standard email
/// format. Instances can only be created through the Create method, which performs validation. Email is
/// case-insensitive and always stored in lowercase. This type is intended for use in domain models where value object
/// semantics and email validation are required.</remarks>
public sealed class Email : ValueObject
{
    /// <summary>
    /// Represents a compiled regular expression used to validate email address formats.
    /// </summary>
    /// <remarks>The regular expression checks for a basic pattern of email addresses, including alphanumeric
    /// characters, periods, underscores, and other common symbols before the '@' symbol, followed by a domain name and
    /// a top-level domain of at least two characters. This pattern may not cover all valid email addresses as defined
    /// by RFC standards, but is suitable for most common use cases.</remarks>
    private static readonly Regex EmailRegex = new(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", RegexOptions.Compiled);

    /// <summary>
    /// Gets the value represented by this instance.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the Email class with the specified email address value.
    /// </summary>
    /// <param name="value">The email address to assign to this instance. Cannot be null or empty.</param>
    private Email(string value)
    {
        // Validate that the email is not null, empty, or whitespace.
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Email cannot be empty");
        }
        // Validate the email format using a helper method.
        if (!IsValidEmail(value))
        {
            throw new ArgumentException("Invalid email format");
        }
        // The constructor is private to enforce the use of the Create method for validation.
        Value = value;
    }

    /// <summary>
    /// Creates a new <see cref="Email"/> instance from the specified email address, validating its format.
    /// </summary>
    /// <param name="email">The email address to validate and use for creating the <see cref="Email"/> instance. Cannot be null, empty, or
    /// whitespace.</param>
    /// <returns>A <see cref="Result{T}"/> containing the created <see cref="Email"/> if the email address is valid; otherwise, a
    /// failure result with an error message.</returns>
    public static Result<Email> Create(string email)
    {
        // Validate that the email is not null, empty, or whitespace.
        if (string.IsNullOrWhiteSpace(email))
        {
            // Return a failure result if the email is invalid.
            return Result.Failure<Email>("Email cannot be empty.");
        }
        // Normalize the email to lowercase for case-insensitive comparison.
        email = email.Trim().ToLowerInvariant();
        // Validate the email format using the regular expression.
        if (!EmailRegex.IsMatch(email))
        {
            // Return a failure result if the email format is invalid.
            return Result.Failure<Email>("Email format is invalid.");
        }
        // Return a success result with the created Email instance.
        return Result.Success(new Email(email));
    }

    /// <summary>
    /// Provides the components that are used to determine equality for this instance.
    /// </summary>
    /// <remarks>Override this method in derived classes to specify which fields or properties should
    /// participate in value-based equality comparisons. The returned components are compared in order to determine
    /// whether two instances are equal.</remarks>
    /// <returns>An enumerable collection of objects that represent the values to be used for equality comparison.</returns>
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        // Return the email value as the sole component for equality comparison.
        yield return Value;
    }

    /// <summary>
    /// Returns the string representation of the current object.
    /// </summary>
    /// <returns>A string that represents the value of the current object.</returns>
    public override string ToString() => Value;

    /// <summary>
    /// Converts an Email instance to its string representation.
    /// </summary>
    /// <remarks>This operator enables implicit conversion of an Email object to a string, returning the
    /// underlying email address value. This allows Email instances to be used in contexts where a string is
    /// expected.</remarks>
    /// <param name="email">The Email instance to convert to a string.</param>
    public static implicit operator string(Email email) => email.Value;

    /// <summary>
    /// Determines whether the specified string is a valid email address format.
    /// </summary>
    /// <remarks>This method checks the format of the email address but does not verify that the address
    /// exists or is reachable.</remarks>
    /// <param name="email">The email address to validate. Cannot be null.</param>
    /// <returns>true if the specified string is a valid email address format; otherwise, false.</returns>
    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
