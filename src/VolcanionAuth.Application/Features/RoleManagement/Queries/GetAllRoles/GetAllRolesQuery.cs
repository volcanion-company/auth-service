using VolcanionAuth.Application.Features.RoleManagement.Common;

namespace VolcanionAuth.Application.Features.RoleManagement.Queries.GetAllRoles;

/// <summary>
/// Represents a query to retrieve a paginated list of roles, optionally including inactive roles and filtering by a
/// search term.
/// </summary>
/// <param name="Page">The page number of results to retrieve. Must be greater than or equal to 1.</param>
/// <param name="PageSize">The maximum number of roles to include in a single page of results. Must be greater than 0.</param>
/// <param name="IncludeInactive">A value indicating whether inactive roles should be included in the results. Specify <see langword="true"/> to
/// include inactive roles; otherwise, only active roles are returned.</param>
/// <param name="SearchTerm">An optional search term used to filter roles by name or description. If <see langword="null"/> or empty, no
/// filtering is applied.</param>
public record GetAllRolesQuery(
    int Page = 1,
    int PageSize = 10,
    bool IncludeInactive = false,
    string? SearchTerm = null
) : IRequest<Result<PaginatedRoleResponse>>;

/// <summary>
/// Represents a paginated response containing a collection of roles and pagination metadata.
/// </summary>
/// <remarks>Use this record to retrieve role data in a paginated format, typically for scenarios where the
/// complete set of roles is too large to return in a single response. Pagination metadata can be used to implement
/// navigation controls in user interfaces.</remarks>
/// <param name="Roles">The list of roles included in the current page of results. The list may be empty if no roles are available for the
/// specified page.</param>
/// <param name="CurrentPage">The zero-based index of the current page in the paginated result set. Must be greater than or equal to 0.</param>
/// <param name="PageSize">The maximum number of roles returned per page. Must be greater than 0.</param>
/// <param name="TotalCount">The total number of roles available across all pages.</param>
/// <param name="TotalPages">The total number of pages available based on the page size and total count.</param>
public record PaginatedRoleResponse(
    List<RoleDto> Roles,
    int CurrentPage,
    int PageSize,
    int TotalCount,
    int TotalPages
);
