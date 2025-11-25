using MediatR;
using VolcanionAuth.Domain.Common;

namespace VolcanionAuth.Application.Features.PermissionManagement.Queries.GetPermissionsGroupedByResource;

public record GetPermissionsGroupedByResourceQuery : IRequest<Result<List<PermissionsByResourceDto>>>;

public record PermissionsByResourceDto(
    string Resource,
    List<PermissionItemDto> Permissions
);

public record PermissionItemDto(
    Guid Id,
    string Action,
    string? Description,
    string PermissionString,
    DateTime CreatedAt
);
