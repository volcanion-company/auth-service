using MediatR;
using VolcanionAuth.Domain.Common;

namespace VolcanionAuth.Application.Features.Authorization.Queries.GetUserPermissions;

public record GetUserPermissionsQuery(Guid UserId) : IRequest<Result<GetUserPermissionsResponse>>;

public record GetUserPermissionsResponse(
    Guid UserId,
    List<string> Roles,
    List<PermissionDto> Permissions
);

public record PermissionDto(
    Guid Id,
    string Resource,
    string Action,
    string PermissionString
);
