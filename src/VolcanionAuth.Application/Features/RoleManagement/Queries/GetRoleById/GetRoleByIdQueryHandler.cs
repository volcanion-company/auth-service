using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Application.Features.RoleManagement.Common;
using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Application.Features.RoleManagement.Queries.GetRoleById;

/// <summary>
/// Handles queries to retrieve a role by its unique identifier, including its associated permissions.
/// </summary>
/// <remarks>This handler maps the retrieved role entity to a data transfer object (DTO) containing role details
/// and permissions. If the specified role does not exist, the result will indicate failure. This handler is typically
/// used in scenarios where role information, including permissions, is required for authorization or management
/// purposes.</remarks>
/// <param name="roleRepository">The repository used to access role data and permissions from the data store.</param>
public class GetRoleByIdQueryHandler(IReadRepository<Role> roleRepository) : IRequestHandler<GetRoleByIdQuery, Result<RoleDto>>
{
    /// <summary>
    /// Handles the retrieval of a role and its associated permissions by role identifier.
    /// </summary>
    /// <remarks>The returned <see cref="RoleDto"/> includes all permissions assigned to the role. If the
    /// specified role does not exist, the result will indicate failure and contain an appropriate error
    /// message.</remarks>
    /// <param name="request">The query containing the role identifier to retrieve. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A result containing the role and its permissions as a <see cref="RoleDto"/> if found; otherwise, a failure
    /// result indicating that the role was not found.</returns>
    public async Task<Result<RoleDto>> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
    {
        // Find the role by ID with permissions
        var role = await roleRepository.GetRoleWithPermissionsAsync(request.RoleId, cancellationToken);

        if (role == null)
        {
            return Result.Failure<RoleDto>($"Role with ID '{request.RoleId}' was not found");
        }

        // Map to DTO
        var roleDto = new RoleDto(
            role.Id,
            role.Name,
            role.Description,
            role.IsActive,
            role.CreatedAt,
            role.UpdatedAt,
            [.. role.RolePermissions.Select(rp => new RolePermissionDto(
                rp.PermissionId,
                rp.Permission.Resource,
                rp.Permission.Action,
                rp.Permission.GetPermissionString()
            ))]
        );

        return Result.Success(roleDto);
    }
}
