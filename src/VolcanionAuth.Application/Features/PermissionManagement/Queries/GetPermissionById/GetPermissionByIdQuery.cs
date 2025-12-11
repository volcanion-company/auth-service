using VolcanionAuth.Application.Features.PermissionManagement.Common;

namespace VolcanionAuth.Application.Features.PermissionManagement.Queries.GetPermissionById;

/// <summary>
/// Represents a query to retrieve a permission by its unique identifier.
/// </summary>
/// <param name="PermissionId">The unique identifier of the permission to retrieve.</param>
public record GetPermissionByIdQuery(Guid PermissionId) : IRequest<Result<PermissionDto>>;
