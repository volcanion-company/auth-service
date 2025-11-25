using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Domain.Entities;
using RefreshTokenEntity = VolcanionAuth.Domain.Entities.RefreshToken;

namespace VolcanionAuth.Application.Features.Authentication.Commands.RefreshToken;

/// <summary>
/// Handles the refresh token command by validating the refresh token and generating new access and refresh tokens.
/// </summary>
public class RefreshTokenCommandHandler(
    IReadRepository<RefreshTokenEntity> refreshTokenReadRepository,
    IReadRepository<User> userReadRepository,
    IJwtTokenService jwtTokenService,
    IUnitOfWork unitOfWork) : IRequestHandler<RefreshTokenCommand, Result<RefreshTokenResponse>>
{
    public async Task<Result<RefreshTokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // Find the refresh token
        var refreshToken = await refreshTokenReadRepository.GetRefreshTokenAsync(request.RefreshToken, cancellationToken);
        if (refreshToken == null)
        {
            return Result.Failure<RefreshTokenResponse>("Invalid refresh token.");
        }

        // Validate refresh token status
        if (refreshToken.IsRevoked)
        {
            return Result.Failure<RefreshTokenResponse>("Refresh token has been revoked.");
        }

        if (refreshToken.IsExpired)
        {
            return Result.Failure<RefreshTokenResponse>("Refresh token has expired.");
        }

        // Get user with permissions
        var user = await userReadRepository.GetUserWithPermissionsAsync(refreshToken.UserId, cancellationToken);
        if (user == null)
        {
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
