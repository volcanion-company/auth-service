using System.Text.RegularExpressions;
using VolcanionAuth.Domain.Common;

namespace VolcanionAuth.Domain.ValueObjects;

/// <summary>
/// Email value object with validation
/// </summary>
public sealed class Email : ValueObject
{
    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled);

    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Result<Email> Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure<Email>("Email cannot be empty.");

        email = email.Trim().ToLowerInvariant();

        if (!EmailRegex.IsMatch(email))
            return Result.Failure<Email>("Email format is invalid.");

        return Result.Success(new Email(email));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;
}
