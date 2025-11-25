namespace VolcanionAuth.Application.Features.Authentication.Commands.RefreshToken;

/// <summary>
/// Represents the response returned after successfully refreshing an access token.
/// </summary>
/// <param name="AccessToken">The new JWT access token for authenticated API requests.</param>
/// <param name="RefreshToken">The new refresh token to use for future token refresh requests.</param>
/// <param name="ExpiresAt">The date and time when the new refresh token expires.</param>
public record RefreshTokenResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt
);
