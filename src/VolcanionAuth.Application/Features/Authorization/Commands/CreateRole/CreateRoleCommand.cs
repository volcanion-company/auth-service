using MediatR;
using VolcanionAuth.Domain.Common;

namespace VolcanionAuth.Application.Features.Authorization.Commands.CreateRole;

public record CreateRoleCommand(
    string Name,
    string? Description
) : IRequest<Result<CreateRoleResponse>>;

public record CreateRoleResponse(
    Guid RoleId,
    string Name
);
