using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Domain.Entities;
using RefreshTokenEntity = VolcanionAuth.Domain.Entities.RefreshToken;

namespace VolcanionAuth.Application.Features.Authentication.Commands.RefreshToken;

/// <summary>
/// Handles refresh token commands by validating the provided refresh token, revoking it if valid, and issuing new
/// access and refresh tokens for the user.
/// </summary>
/// <remarks>This handler ensures that only valid, non-expired, and non-revoked refresh tokens are accepted. It
/// also verifies that the associated user account is active before issuing new tokens. The handler is typically used in
/// authentication workflows to support secure token renewal.</remarks>
/// <param name="refreshTokenReadRepository">The repository used to retrieve and validate refresh token entities from the data store.</param>
/// <param name="userReadRepository">The repository used to retrieve user information and associated permissions required for token generation.</param>
/// <param name="jwtTokenService">The service responsible for generating new JWT access and refresh tokens.</param>
/// <param name="unitOfWork">The unit of work used to persist changes to the data store, such as revoking tokens and updating user entities.</param>
public class RefreshTokenCommandHandler(
    IReadRepository<RefreshTokenEntity> refreshTokenReadRepository,
    IReadRepository<User> userReadRepository,
    IJwtTokenService jwtTokenService,
    IUnitOfWork unitOfWork) : IRequestHandler<RefreshTokenCommand, Result<RefreshTokenResponse>>
{
    /// <summary>
    /// Processes a refresh token request and issues a new access token and refresh token if the provided refresh token
    /// and user are valid.
    /// </summary>
    /// <remarks>The method validates the provided refresh token and associated user before issuing new
    /// tokens. The operation will fail if the refresh token is invalid, revoked, expired, or if the user is not found
    /// or inactive.</remarks>
    /// <param name="request">The refresh token command containing the refresh token to validate and refresh.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A result containing a <see cref="RefreshTokenResponse"/> with the new access and refresh tokens if the operation
    /// succeeds; otherwise, a failure result with an error message.</returns>
    public async Task<Result<RefreshTokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // Find the refresh token
        var refreshToken = await refreshTokenReadRepository.GetRefreshTokenAsync(request.RefreshToken, cancellationToken);
        if (refreshToken == null)
        {
            // Invalid refresh token
            return Result.Failure<RefreshTokenResponse>("Invalid refresh token.");
        }
        // Validate refresh token status
        if (refreshToken.IsRevoked)
        {
            // Revoked refresh token
            return Result.Failure<RefreshTokenResponse>("Refresh token has been revoked.");
        }
        // Expired refresh token
        if (refreshToken.IsExpired)
        {
            return Result.Failure<RefreshTokenResponse>("Refresh token has expired.");
        }

        // Get user with permissions
        var user = await userReadRepository.GetUserWithPermissionsAsync(refreshToken.UserId, cancellationToken);
        if (user == null)
        {
            // User not found
            return Result.Failure<RefreshTokenResponse>("User not found.");
        }

        // Check if user is active
        if (!user.IsActive)
        {
            return Result.Failure<RefreshTokenResponse>("User account is not active.");
        }

        // Get user permissions
        var permissions = await userReadRepository.GetUserPermissionsAsync(user.Id, cancellationToken);
        var roles = user.UserRoles.Select(ur => ur.RoleId.ToString()).ToList();
        var permissionStrings = permissions.Select(p => p.GetPermissionString()).ToList();

        // Generate new tokens
        var newAccessToken = jwtTokenService.GenerateAccessToken(user, roles, permissionStrings);
        var newRefreshToken = jwtTokenService.GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddDays(7);

        // Revoke old refresh token and create new one
        refreshToken.Revoke();
        user.CreateRefreshToken(newRefreshToken, expiresAt);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new RefreshTokenResponse(
            newAccessToken,
            newRefreshToken,
            expiresAt
        );

        return Result.Success(response);
    }
}
