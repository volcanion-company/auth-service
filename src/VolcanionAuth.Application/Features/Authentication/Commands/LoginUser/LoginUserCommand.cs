using MediatR;
using VolcanionAuth.Domain.Common;

namespace VolcanionAuth.Application.Features.Authentication.Commands.LoginUser;

/// <summary>
/// Command to login user
/// </summary>
public record LoginUserCommand(
    string Email,
    string Password,
    string IpAddress,
    string UserAgent
) : IRequest<Result<LoginUserResponse>>;

public record LoginUserResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    Guid UserId,
    string Email,
    string FullName
);
