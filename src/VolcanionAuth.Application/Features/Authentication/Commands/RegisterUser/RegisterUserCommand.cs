namespace VolcanionAuth.Application.Features.Authentication.Commands.RegisterUser;

/// <summary>
/// Represents a command to register a new user with the specified credentials and personal information.
/// </summary>
/// <remarks>This command is typically used in a request-response pattern to initiate user registration. The
/// result indicates whether registration was successful and may include additional information about the newly
/// registered user.</remarks>
/// <param name="Email">The email address of the user to register. Must be a valid, non-empty email address.</param>
/// <param name="Password">The password for the new user account. Must meet the application's password requirements.</param>
/// <param name="FirstName">The first name of the user to register. Cannot be null or empty.</param>
/// <param name="LastName">The last name of the user to register. Cannot be null or empty.</param>
public record RegisterUserCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName
) : IRequest<Result<RegisterUserResponse>>;

/// <summary>
/// Represents the result of a user registration operation, including the newly created user's identifier and profile
/// information.
/// </summary>
/// <param name="UserId">The unique identifier assigned to the newly registered user.</param>
/// <param name="Email">The email address associated with the registered user.</param>
/// <param name="FullName">The full name of the registered user.</param>
public record RegisterUserResponse(
    Guid UserId,
    string Email,
    string FullName
);
