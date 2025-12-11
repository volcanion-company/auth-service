namespace VolcanionAuth.Application.Features.Authentication.Commands.RefreshToken;

/// <summary>
/// Represents the response returned after successfully refreshing an authentication token, including the new access
/// token, refresh token, and expiration time.
/// </summary>
/// <param name="AccessToken">The newly issued access token to be used for authenticated requests.</param>
/// <param name="RefreshToken">The refresh token that can be used to obtain future access tokens when the current one expires.</param>
/// <param name="ExpiresAt">The date and time, in UTC, when the access token expires.</param>
public record RefreshTokenResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt
);
