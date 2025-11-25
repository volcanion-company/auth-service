using MediatR;
using VolcanionAuth.Domain.Common;

namespace VolcanionAuth.Application.Features.Authentication.Commands.RefreshToken;

/// <summary>
/// Represents a command to refresh an access token using a valid refresh token.
/// </summary>
/// <param name="RefreshToken">The refresh token to validate and use for generating a new access token.</param>
public record RefreshTokenCommand(
    string RefreshToken
) : IRequest<Result<RefreshTokenResponse>>;
