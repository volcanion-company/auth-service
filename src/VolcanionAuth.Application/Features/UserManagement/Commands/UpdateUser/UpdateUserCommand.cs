namespace VolcanionAuth.Application.Features.UserManagement.Commands.UpdateUser;

/// <summary>
/// Represents a command to update an existing user's profile information, including email address and optional personal
/// details.
/// </summary>
/// <remarks>All fields except <paramref name="UserId"/> and <paramref name="Email"/> are optional. Only provided
/// values will be updated; omitted fields will remain unchanged.</remarks>
/// <param name="UserId">The unique identifier of the user whose information is to be updated.</param>
/// <param name="Email">The new email address to assign to the user. Cannot be null or empty.</param>
/// <param name="FirstName">The updated first name of the user, or null to leave unchanged.</param>
/// <param name="LastName">The updated last name of the user, or null to leave unchanged.</param>
/// <param name="PhoneNumber">The updated phone number of the user, or null to leave unchanged.</param>
public record UpdateUserCommand(
    Guid UserId,
    string Email,
    string? FirstName = null,
    string? LastName = null,
    string? PhoneNumber = null
) : IRequest<Result<UpdateUserResponse>>;

/// <summary>
/// Represents the result of a user update operation, including the user's identifier and updated profile information.
/// </summary>
/// <param name="UserId">The unique identifier of the user whose information was updated.</param>
/// <param name="Email">The updated email address of the user.</param>
/// <param name="FirstName">The updated first name of the user.</param>
/// <param name="LastName">The updated last name of the user.</param>
public record UpdateUserResponse(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName
);
