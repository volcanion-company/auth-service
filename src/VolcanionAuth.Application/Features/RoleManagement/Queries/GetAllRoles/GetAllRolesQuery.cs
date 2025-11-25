using VolcanionAuth.Application.Features.RoleManagement.Common;

namespace VolcanionAuth.Application.Features.RoleManagement.Queries.GetAllRoles;

/// <summary>
/// Query to retrieve a paginated list of all roles in the system.
/// </summary>
public record GetAllRolesQuery(
    int Page = 1,
    int PageSize = 10,
    bool IncludeInactive = false,
    string? SearchTerm = null
) : IRequest<Result<PaginatedRoleResponse>>;

/// <summary>
/// Paginated response containing a list of roles.
/// </summary>
public record PaginatedRoleResponse(
    List<RoleDto> Roles,
    int CurrentPage,
    int PageSize,
    int TotalCount,
    int TotalPages
);
