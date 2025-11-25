using VolcanionAuth.Domain.Common;

namespace VolcanionAuth.Domain.ValueObjects;

/// <summary>
/// Full name value object
/// </summary>
public sealed class FullName : ValueObject
{
    public string FirstName { get; }
    public string LastName { get; }

    private FullName(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    public static Result<FullName> Create(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            return Result.Failure<FullName>("First name cannot be empty.");

        if (string.IsNullOrWhiteSpace(lastName))
            return Result.Failure<FullName>("Last name cannot be empty.");

        if (firstName.Length > 50)
            return Result.Failure<FullName>("First name cannot exceed 50 characters.");

        if (lastName.Length > 50)
            return Result.Failure<FullName>("Last name cannot exceed 50 characters.");

        return Result.Success(new FullName(firstName.Trim(), lastName.Trim()));
    }

    public string GetFullName() => $"{FirstName} {LastName}";

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return FirstName;
        yield return LastName;
    }

    public override string ToString() => GetFullName();
}
