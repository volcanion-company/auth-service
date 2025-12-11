namespace VolcanionAuth.Application.Features.Authentication.Commands.LoginUser;

/// <summary>
/// Represents a command to authenticate a user using their email address and password, including client context
/// information for the login attempt.
/// </summary>
/// <remarks>This command is typically used in authentication workflows to initiate a user login and may be
/// processed by a handler that validates credentials and returns a result indicating success or failure. Ensure that
/// sensitive information such as passwords is handled securely.</remarks>
/// <param name="Email">The email address of the user attempting to log in. Cannot be null or empty.</param>
/// <param name="Password">The password associated with the specified email address. Cannot be null or empty.</param>
/// <param name="IpAddress">The IP address from which the login request originated. Used for auditing and security purposes.</param>
/// <param name="UserAgent">The user agent string identifying the client's browser or application. Used for logging and security analysis.</param>
public record LoginUserCommand(
    string Email,
    string Password,
    string IpAddress,
    string UserAgent
) : IRequest<Result<LoginUserResponse>>;

/// <summary>
/// Represents the response returned after a successful user login, containing authentication tokens and user
/// information.
/// </summary>
/// <param name="AccessToken">The JWT access token issued to the authenticated user. Used to authorize subsequent API requests.</param>
/// <param name="RefreshToken">The token that can be used to obtain a new access token when the current one expires.</param>
/// <param name="ExpiresAt">The date and time at which the access token expires, in UTC.</param>
/// <param name="UserId">The unique identifier of the authenticated user.</param>
/// <param name="Email">The email address associated with the authenticated user.</param>
/// <param name="FullName">The full name of the authenticated user.</param>
public record LoginUserResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    Guid UserId,
    string Email,
    string FullName
);
