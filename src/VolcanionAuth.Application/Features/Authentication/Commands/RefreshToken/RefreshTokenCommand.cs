namespace VolcanionAuth.Application.Features.Authentication.Commands.RefreshToken;

/// <summary>
/// Represents a request to obtain a new access token using a refresh token.
/// </summary>
/// <param name="RefreshToken">The refresh token used to authenticate the request and generate a new access token. Cannot be null or empty.</param>
public record RefreshTokenCommand(string RefreshToken) : IRequest<Result<RefreshTokenResponse>>;
