using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Application.Features.PermissionManagement.Common;
using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Application.Features.PermissionManagement.Queries.GetAllPermissions;

/// <summary>
/// Handles queries to retrieve all permissions, grouped by resource, with support for filtering and pagination.
/// </summary>
/// <remarks>This handler supports filtering permissions by resource and search term, and paginates results based
/// on resource groups. It is typically used to provide a paginated, grouped view of permissions for administrative or
/// management interfaces.</remarks>
/// <param name="permissionRepository">The repository used to access and retrieve permission entities from the data store.</param>
public class GetAllPermissionsQueryHandler(IReadRepository<Permission> permissionRepository) : IRequestHandler<GetAllPermissionsQuery, Result<PaginatedPermissionResponse>>
{
    /// <summary>
    /// Retrieves a paginated list of permissions grouped by resource, optionally filtered by resource name and search
    /// term.
    /// </summary>
    /// <remarks>The returned permissions are grouped by resource and sorted alphabetically. Filtering by
    /// resource and search term is case-insensitive. Pagination is applied to resource groups, not individual
    /// permissions.</remarks>
    /// <param name="request">The query parameters specifying pagination, resource filtering, and search criteria for retrieving permissions.
    /// The page number must be greater than 0, and the page size must be between 1 and 100.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A result containing a paginated response with permissions grouped by resource. Returns a failure result if
    /// pagination parameters are invalid.</returns>
    public async Task<Result<PaginatedPermissionResponse>> Handle(GetAllPermissionsQuery request, CancellationToken cancellationToken)
    {
        // Validate pagination parameters
        if (request.Page < 1)
        {
            return Result.Failure<PaginatedPermissionResponse>("Page number must be greater than 0");
        }

        if (request.PageSize < 1 || request.PageSize > 100)
        {
            return Result.Failure<PaginatedPermissionResponse>("Page size must be between 1 and 100");
        }

        // Get all permissions
        var allPermissions = await permissionRepository.GetAllAsync(cancellationToken);

        // Filter by resource if provided
        var filteredPermissions = allPermissions.ToList();
        if (!string.IsNullOrWhiteSpace(request.Resource))
        {
            filteredPermissions = [.. filteredPermissions.Where(p => p.Resource.Equals(request.Resource, StringComparison.OrdinalIgnoreCase))];
        }

        // Filter by search term if provided
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchLower = request.SearchTerm.ToLower();
            filteredPermissions = [.. filteredPermissions
                .Where(p => p.Resource.Contains(searchLower, StringComparison.CurrentCultureIgnoreCase) || 
                           p.Action.Contains(searchLower, StringComparison.CurrentCultureIgnoreCase) ||
                           (p.Description != null && p.Description.Contains(searchLower, StringComparison.CurrentCultureIgnoreCase)))];
        }

        // Group permissions by resource and sort
        var groupedPermissions = filteredPermissions
            .GroupBy(p => p.Resource)
            .OrderBy(g => g.Key)
            .Select(g => new
            {
                Resource = g.Key,
                Permissions = g.OrderBy(p => p.Action).ToList()
            })
            .ToList();

        // Calculate pagination based on resource groups
        var totalCount = groupedPermissions.Count;
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        // Apply pagination on resource groups
        var paginatedGroups = groupedPermissions
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        // Map to DTOs
        var permissionsByResource = paginatedGroups.Select(g => new PermissionsByResourceDto(
            g.Resource,
            [.. g.Permissions.Select(p => new PermissionDto(
                p.Id,
                p.Resource,
                p.Action,
                p.GetPermissionString(),
                p.Description,
                p.CreatedAt,
                p.RolePermissions.Count
            ))]
        )).ToList();

        var response = new PaginatedPermissionResponse(
            permissionsByResource,
            request.Page,
            request.PageSize,
            totalCount,
            totalPages
        );

        return Result.Success(response);
    }
}
