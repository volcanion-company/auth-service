using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Application.Features.PermissionManagement.Common;
using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Application.Features.PermissionManagement.Queries.GetPermissionById;

/// <summary>
/// Handles queries to retrieve a permission by its unique identifier and returns the corresponding permission data
/// transfer object (DTO).
/// </summary>
/// <param name="permissionRepository">The repository used to access and retrieve permission entities from the data store.</param>
public class GetPermissionByIdQueryHandler(IReadRepository<Permission> permissionRepository) : IRequestHandler<GetPermissionByIdQuery, Result<PermissionDto>>
{
    /// <summary>
    /// Handles a query to retrieve a permission by its unique identifier.
    /// </summary>
    /// <param name="request">The query containing the permission ID to retrieve.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A result containing the permission details as a <see cref="PermissionDto"/> if found; otherwise, a failure
    /// result indicating that the permission was not found.</returns>
    public async Task<Result<PermissionDto>> Handle(GetPermissionByIdQuery request, CancellationToken cancellationToken)
    {
        // Find the permission by ID
        var permission = await permissionRepository.GetByIdAsync(request.PermissionId, cancellationToken);
        if (permission == null)
        {
            // Permission not found
            return Result.Failure<PermissionDto>($"Permission with ID '{request.PermissionId}' was not found");
        }

        // Map to DTO
        var permissionDto = new PermissionDto(
            permission.Id,
            permission.Resource,
            permission.Action,
            permission.GetPermissionString(),
            permission.Description,
            permission.CreatedAt,
            permission.RolePermissions.Count
        );
        // Return success result with DTO
        return Result.Success(permissionDto);
    }
}
