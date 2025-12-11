using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Application.Features.Authentication.Commands.LoginUser;

/// <summary>
/// Handles user login requests by validating credentials, recording login attempts, generating authentication tokens,
/// and caching the user session.
/// </summary>
/// <remarks>This handler coordinates multiple services to ensure secure authentication and session management. It
/// records both successful and failed login attempts, and caches session data to optimize subsequent requests. Thread
/// safety and transactional integrity are maintained through the use of the unit of work pattern.</remarks>
/// <param name="userRepository">The user repository used to retrieve and update user entities, including roles and login history.</param>
/// <param name="readRepository">The read-only repository for accessing user permissions and related data.</param>
/// <param name="passwordHasher">The service used to verify user passwords against stored password hashes.</param>
/// <param name="jwtTokenService">The service responsible for generating access and refresh JWT tokens for authenticated users.</param>
/// <param name="unitOfWork">The unit of work used to persist changes to the data store as part of the login process.</param>
/// <param name="cacheService">The cache service used to store user session information for improved performance and scalability.</param>
public class LoginUserCommandHandler(
    IUserRepository userRepository,
    IReadRepository<User> readRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService,
    IUnitOfWork unitOfWork,
    ICacheService cacheService) : IRequestHandler<LoginUserCommand, Result<LoginUserResponse>>
{
    /// <summary>
    /// Authenticates a user based on the provided login credentials and returns a result containing authentication
    /// tokens and user information.
    /// </summary>
    /// <remarks>This method normalizes the email for case-insensitive comparison and records both successful
    /// and failed login attempts. If authentication is successful, user session data is cached for 30 minutes. The
    /// returned tokens are valid for 7 days. The method does not throw exceptions for invalid credentials; instead, it
    /// returns a failure result.</remarks>
    /// <param name="request">The login command containing the user's email, password, IP address, and user agent information used for
    /// authentication.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the authentication operation.</param>
    /// <returns>A result containing a <see cref="LoginUserResponse"/> with access and refresh tokens, token expiration, and user
    /// details if authentication succeeds; otherwise, a failure result with an error message.</returns>
    public async Task<Result<LoginUserResponse>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        // Normalize email for case-insensitive comparison
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        // Get user from write repository with roles (tracked entity)
        var user = await userRepository.GetUserByEmailWithRolesAsync(normalizedEmail, cancellationToken);
        if (user == null)
        {
            // Record failed login attempt for non-existing user
            return Result.Failure<LoginUserResponse>("Invalid credentials.");
        }

        // Verify password
        if (!passwordHasher.VerifyPassword(request.Password, user.Password.Hash))
        {
            // Record failed login
            user.RecordFailedLogin(request.IpAddress, request.UserAgent);
            // Save changes for failed login attempt
            await unitOfWork.SaveChangesAsync(cancellationToken);
            // Return generic error message
            return Result.Failure<LoginUserResponse>("Invalid credentials.");
        }

        // Record successful login
        var loginResult = user.RecordSuccessfulLogin(request.IpAddress, request.UserAgent);
        if (loginResult.IsFailure)
        {
            // Return failure if recording login failed
            return Result.Failure<LoginUserResponse>(loginResult.Error);
        }

        // Get user permissions from read repository
        var permissions = await readRepository.GetUserPermissionsAsync(user.Id, cancellationToken);
        
        // Get roles from the tracked user entity (already loaded with Include)
        var roles = user.UserRoles.Select(ur => ur.RoleId.ToString()).ToList();
        var permissionStrings = permissions.Select(p => p.GetPermissionString()).ToList();

        // Generate tokens
        var accessToken = jwtTokenService.GenerateAccessToken(user, roles, permissionStrings);
        var refreshToken = jwtTokenService.GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddDays(7);

        // Create refresh token
        user.CreateRefreshToken(refreshToken, expiresAt);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Cache user session
        await cacheService.SetAsync($"user_session:{user.Id}", new
        {
            user.Id,
            user.Email,
            Roles = roles,
            Permissions = permissionStrings
        }, TimeSpan.FromMinutes(30), cancellationToken);

        var response = new LoginUserResponse(
            accessToken,
            refreshToken,
            expiresAt,
            user.Id,
            user.Email,
            user.FullName.GetFullName()
        );

        return Result.Success(response);
    }
}
