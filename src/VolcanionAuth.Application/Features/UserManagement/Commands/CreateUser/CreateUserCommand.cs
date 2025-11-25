using MediatR;
using VolcanionAuth.Domain.Common;

namespace VolcanionAuth.Application.Features.UserManagement.Commands.CreateUser;

/// <summary>
/// Command to create a new user in the system.
/// </summary>
/// <param name="Email">User's email address</param>
/// <param name="Password">User's password (will be hashed)</param>
/// <param name="FirstName">User's first name</param>
/// <param name="LastName">User's last name</param>
/// <param name="PhoneNumber">User's phone number (optional)</param>
/// <param name="RoleIds">List of role IDs to assign to the user (optional)</param>
public record CreateUserCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? PhoneNumber = null,
    List<Guid>? RoleIds = null
) : IRequest<Result<CreateUserResponse>>;

/// <summary>
/// Response object containing the created user's information.
/// </summary>
public record CreateUserResponse(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName
);
