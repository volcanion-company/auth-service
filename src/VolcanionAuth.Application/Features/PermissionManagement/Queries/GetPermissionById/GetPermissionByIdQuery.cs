using VolcanionAuth.Application.Features.PermissionManagement.Common;

namespace VolcanionAuth.Application.Features.PermissionManagement.Queries.GetPermissionById;

/// <summary>
/// Query to retrieve detailed information about a specific permission.
/// </summary>
public record GetPermissionByIdQuery(Guid PermissionId) : IRequest<Result<PermissionDto>>;
