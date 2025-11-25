using MediatR;
using VolcanionAuth.Domain.Common;

namespace VolcanionAuth.Application.Features.Authorization.Commands.AssignRole;

public record AssignRoleCommand(
    Guid UserId,
    Guid RoleId
) : IRequest<Result>;
