using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Application.Features.PermissionManagement.Common;
using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Application.Features.PermissionManagement.Queries.GetAllPermissions;

/// <summary>
/// Handler for retrieving a paginated list of all permissions.
/// </summary>
public class GetAllPermissionsQueryHandler(IReadRepository<Permission> permissionRepository) : IRequestHandler<GetAllPermissionsQuery, Result<PaginatedPermissionResponse>>
{
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
            g.Permissions.Select(p => new PermissionDto(
                p.Id,
                p.Resource,
                p.Action,
                p.GetPermissionString(),
                p.Description,
                p.CreatedAt,
                p.RolePermissions.Count
            )).ToList()
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
