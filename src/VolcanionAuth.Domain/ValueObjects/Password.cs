using VolcanionAuth.Domain.Common;

namespace VolcanionAuth.Domain.ValueObjects;

/// <summary>
/// Represents a password value object that encapsulates a hashed password and enforces password complexity
/// requirements.
/// </summary>
/// <remarks>The Password class is immutable and can only be created through the provided factory methods. Use the
/// Create method to validate and construct a password from a plain text value, or CreateFromHash to instantiate from an
/// existing hash. This type is intended to ensure that password values meet minimum security standards before being
/// persisted or used.</remarks>
public sealed class Password : ValueObject
{
    /// <summary>
    /// Gets the hash value that uniquely identifies the content or state of the object.
    /// </summary>
    public string Hash { get; }

    /// <summary>
    /// Initializes a new instance of the Password class with the specified password hash.
    /// </summary>
    /// <param name="hash">The hashed password value to associate with this instance. Cannot be null.</param>
    private Password(string hash)
    {
        Hash = hash;
    }

    /// <summary>
    /// Creates a new password instance after validating the specified plain text password against defined security
    /// requirements.
    /// </summary>
    /// <remarks>The password must be at least 8 characters long and contain at least one digit, one uppercase
    /// letter, one lowercase letter, and one special character. This method does not throw exceptions for validation
    /// failures; instead, it returns a failure result with an appropriate error message.</remarks>
    /// <param name="plainPassword">The plain text password to validate and use for creating the password instance. Must meet minimum length and
    /// complexity requirements.</param>
    /// <returns>A result containing the created password if the input meets all validation criteria; otherwise, a failure result
    /// with an error message describing the validation issue.</returns>
    public static Result<Password> Create(string plainPassword)
    {
        // Validate password complexity
        if (string.IsNullOrWhiteSpace(plainPassword))
        {
            // Password is null or empty
            return Result.Failure<Password>("Password cannot be empty.");
        }
        // Check length
        if (plainPassword.Length < 8)
        {
            // Password is too short
            return Result.Failure<Password>("Password must be at least 8 characters long.");
        }
        // Check for digit
        if (!plainPassword.Any(char.IsDigit))
        {
            // Password lacks a digit
            return Result.Failure<Password>("Password must contain at least one digit.");
        }
        // Check for uppercase letter
        if (!plainPassword.Any(char.IsUpper))
        {
            // Password lacks an uppercase letter
            return Result.Failure<Password>("Password must contain at least one uppercase letter.");
        }
        // Check for lowercase letter
        if (!plainPassword.Any(char.IsLower))
        {
            // Password lacks a lowercase letter
            return Result.Failure<Password>("Password must contain at least one lowercase letter.");
        }
        // Check for special character
        if (!plainPassword.Any(ch => "!@#$%^&*()_+-=[]{}|;:,.<>?".Contains(ch)))
        {
            // Password lacks a special character
            return Result.Failure<Password>("Password must contain at least one special character.");
        }
        // All validations passed; create and return the Password instance
        return Result.Success(new Password(plainPassword));
    }

    /// <summary>
    /// Creates a new Password instance from an existing password hash.
    /// </summary>
    /// <param name="hash">The hashed password string to use for creating the Password instance. Cannot be null or empty.</param>
    /// <returns>A Password instance initialized with the specified hash.</returns>
    public static Password CreateFromHash(string hash)
    {
        // Directly create a Password instance from the provided hash
        return new Password(hash);
    }

    /// <summary>
    /// Provides the components that are used to determine equality for the current object.
    /// </summary>
    /// <returns>An enumerable collection containing the values that define object equality. The sequence and values determine
    /// whether two instances are considered equal.</returns>
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        // Return the hash as the sole component for equality comparison
        yield return Hash;
    }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string containing the hash value of the current object.</returns>
    public override string ToString() => Hash;
}
