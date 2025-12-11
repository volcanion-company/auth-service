namespace VolcanionAuth.Application.Features.UserManagement.Commands.CreateUser;

/// <summary>
/// Represents a request to create a new user with the specified credentials and profile information.
/// </summary>
/// <remarks>Use this command to initiate user creation in the system. The result will indicate success or failure
/// and provide details about the created user if successful.</remarks>
/// <param name="Email">The email address of the user to be created. Must be a valid, non-empty email address.</param>
/// <param name="Password">The password for the new user account. Must meet any required password complexity or security policies.</param>
/// <param name="FirstName">The first name of the user.</param>
/// <param name="LastName">The last name of the user.</param>
/// <param name="PhoneNumber">The phone number of the user. Optional; can be null if not provided.</param>
/// <param name="RoleIds">A list of role identifiers to assign to the user. Optional; if null or empty, the user will not be assigned any
/// roles.</param>
public record CreateUserCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? PhoneNumber = null,
    List<Guid>? RoleIds = null
) : IRequest<Result<CreateUserResponse>>;

/// <summary>
/// Represents the result of a user creation operation, including the newly created user's identifier and profile
/// information.
/// </summary>
/// <param name="UserId">The unique identifier assigned to the newly created user.</param>
/// <param name="Email">The email address associated with the new user account.</param>
/// <param name="FirstName">The first name of the newly created user.</param>
/// <param name="LastName">The last name of the newly created user.</param>
public record CreateUserResponse(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName
);
