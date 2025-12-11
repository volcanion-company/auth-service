using VolcanionAuth.Application.Features.PermissionManagement.Common;

namespace VolcanionAuth.Application.Features.PermissionManagement.Queries.GetAllPermissions;

/// <summary>
/// Represents a query to retrieve a paginated list of permissions, optionally filtered by resource and search term.
/// </summary>
/// <param name="Page">The page number of results to retrieve. Must be greater than or equal to 1.</param>
/// <param name="PageSize">The maximum number of permissions to include in a single page. Must be greater than 0.</param>
/// <param name="Resource">An optional resource name to filter permissions. If null, permissions for all resources are included.</param>
/// <param name="SearchTerm">An optional search term used to filter permissions by name or description. If null or empty, no search filtering is
/// applied.</param>
public record GetAllPermissionsQuery(
    int Page = 1,
    int PageSize = 10,
    string? Resource = null,
    string? SearchTerm = null
) : IRequest<Result<PaginatedPermissionResponse>>;

/// <summary>
/// Represents a paginated response containing permission data grouped by resource.
/// </summary>
/// <remarks>Use this type to retrieve permission data in a paginated format, which is useful for efficiently
/// handling large datasets. The values of CurrentPage and PageSize can be used to implement navigation controls in user
/// interfaces.</remarks>
/// <param name="Data">The collection of permission entries for each resource included in the current page.</param>
/// <param name="CurrentPage">The zero-based index of the current page in the paginated result set.</param>
/// <param name="PageSize">The maximum number of items included in each page of the response.</param>
/// <param name="TotalCount">The total number of permission entries available across all pages.</param>
/// <param name="TotalPages">The total number of pages in the paginated result set.</param>
public record PaginatedPermissionResponse(
    List<PermissionsByResourceDto> Data,
    int CurrentPage,
    int PageSize,
    int TotalCount,
    int TotalPages
);

/// <summary>
/// Represents a set of permissions associated with a specific resource.
/// </summary>
/// <param name="Resource">The name or identifier of the resource for which permissions are defined. Cannot be null.</param>
/// <param name="Permissions">The collection of permissions granted for the specified resource. Cannot be null; may be empty if no permissions are
/// assigned.</param>
public record PermissionsByResourceDto(
    string Resource,
    List<PermissionDto> Permissions
);
