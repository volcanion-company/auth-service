namespace VolcanionAuth.Application.Features.UserManagement.Commands.UpdateUser;

/// <summary>
/// Command to update an existing user's information.
/// </summary>
/// <param name="UserId">The ID of the user to update</param>
/// <param name="FirstName">User's first name (optional)</param>
/// <param name="LastName">User's last name (optional)</param>
/// <param name="PhoneNumber">User's phone number (optional)</param>
public record UpdateUserCommand(
    Guid UserId,
    string Email,
    string? FirstName = null,
    string? LastName = null,
    string? PhoneNumber = null
) : IRequest<Result<UpdateUserResponse>>;

/// <summary>
/// Response object containing the updated user's information.
/// </summary>
public record UpdateUserResponse(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName
);
