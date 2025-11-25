using VolcanionAuth.Application.Features.PermissionManagement.Common;

namespace VolcanionAuth.Application.Features.PermissionManagement.Queries.GetAllPermissions;

/// <summary>
/// Query to retrieve a paginated list of all permissions grouped by resource.
/// </summary>
public record GetAllPermissionsQuery(
    int Page = 1,
    int PageSize = 10,
    string? Resource = null,
    string? SearchTerm = null
) : IRequest<Result<PaginatedPermissionResponse>>;

/// <summary>
/// Paginated response containing permissions grouped by resource.
/// </summary>
public record PaginatedPermissionResponse(
    List<PermissionsByResourceDto> Data,
    int CurrentPage,
    int PageSize,
    int TotalCount,
    int TotalPages
);

/// <summary>
/// Permissions grouped by a resource.
/// </summary>
public record PermissionsByResourceDto(
    string Resource,
    List<PermissionDto> Permissions
);
