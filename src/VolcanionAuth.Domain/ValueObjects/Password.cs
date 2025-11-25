using VolcanionAuth.Domain.Common;

namespace VolcanionAuth.Domain.ValueObjects;

/// <summary>
/// Password value object with hashing
/// </summary>
public sealed class Password : ValueObject
{
    public string Hash { get; }

    private Password(string hash)
    {
        Hash = hash;
    }

    public static Result<Password> Create(string plainPassword)
    {
        if (string.IsNullOrWhiteSpace(plainPassword))
            return Result.Failure<Password>("Password cannot be empty.");

        if (plainPassword.Length < 8)
            return Result.Failure<Password>("Password must be at least 8 characters long.");

        if (!plainPassword.Any(char.IsDigit))
            return Result.Failure<Password>("Password must contain at least one digit.");

        if (!plainPassword.Any(char.IsUpper))
            return Result.Failure<Password>("Password must contain at least one uppercase letter.");

        if (!plainPassword.Any(char.IsLower))
            return Result.Failure<Password>("Password must contain at least one lowercase letter.");

        if (!plainPassword.Any(ch => "!@#$%^&*()_+-=[]{}|;:,.<>?".Contains(ch)))
            return Result.Failure<Password>("Password must contain at least one special character.");

        return Result.Success(new Password(plainPassword));
    }

    public static Password CreateFromHash(string hash)
    {
        return new Password(hash);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Hash;
    }

    public override string ToString() => Hash;
}
