using MediatR;
using VolcanionAuth.Domain.Common;

namespace VolcanionAuth.Application.Features.Authentication.Commands.RegisterUser;

/// <summary>
/// Command to register a new user
/// </summary>
public record RegisterUserCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName
) : IRequest<Result<RegisterUserResponse>>;

public record RegisterUserResponse(
    Guid UserId,
    string Email,
    string FullName
);
