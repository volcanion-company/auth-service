using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Application.Features.RoleManagement.Common;
using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Application.Features.RoleManagement.Commands.GrantPermissions;

/// <summary>
/// Handles the grant permissions operation for a role by replacing all existing permissions with the new set.
/// </summary>
/// <remarks>This handler validates the existence of the role and permissions before granting them. It clears
/// all existing permissions and assigns the new ones atomically using the provided unit of work.</remarks>
/// <param name="roleRepository">The repository used to persist changes to role entities.</param>
/// <param name="readRoleRepository">The repository used to retrieve role entities for validation and data loading.</param>
/// <param name="permissionRepository">The repository used to access permission entities when granting permissions.</param>
/// <param name="rolePermissionRepository">The repository used to manage role-permission associations.</param>
/// <param name="unitOfWork">The unit of work used to commit changes to the data store.</param>
public class GrantPermissionsCommandHandler(
    IRepository<Role> roleRepository,
    IReadRepository<Role> readRoleRepository,
    IReadRepository<Permission> permissionRepository,
    IRepository<RolePermission> rolePermissionRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<GrantPermissionsCommand, Result<RoleDto>>
{
    /// <summary>
    /// Handles the granting of permissions to a role by replacing all existing permissions with the provided set.
    /// </summary>
    /// <param name="request">The command containing the role ID and the permission IDs to grant.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A result containing the updated role data transfer object if the operation succeeds; otherwise, a failure
    /// result with an error message.</returns>
    public async Task<Result<RoleDto>> Handle(GrantPermissionsCommand request, CancellationToken cancellationToken)
    {
        // Find the role using write repository to ensure it's tracked by the write context
        var role = await roleRepository.GetRoleWithPermissionsAsync(request.RoleId, cancellationToken);
        if (role == null)
        {
            return Result.Failure<RoleDto>($"Role with ID '{request.RoleId}' was not found");
        }

        // Validate that all permission IDs exist
        var allPermissions = await permissionRepository.GetAllAsync(cancellationToken);
        var requestedPermissions = allPermissions.Where(p => request.PermissionIds.Contains(p.Id)).ToList();

        if (requestedPermissions.Count != request.PermissionIds.Count)
        {
            var foundIds = requestedPermissions.Select(p => p.Id).ToList();
            var missingIds = request.PermissionIds.Except(foundIds).ToList();
            return Result.Failure<RoleDto>($"The following permission IDs were not found: {string.Join(", ", missingIds)}");
        }

        // Remove existing permissions using repository to ensure proper tracking
        foreach (var rolePermission in role.RolePermissions.ToList())
        {
            rolePermissionRepository.Remove(rolePermission);
        }

        // Add new permissions directly as new entities
        foreach (var permissionId in request.PermissionIds)
        {
            var newRolePermission = RolePermission.Create(role.Id, permissionId);
            await rolePermissionRepository.AddAsync(newRolePermission, cancellationToken);
        }

        // Update role's UpdatedAt timestamp
        role.Update(role.Name, role.Description);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload role with permissions
        var updatedRole = await readRoleRepository.GetRoleWithPermissionsAsync(request.RoleId, cancellationToken);

        // Map to DTO
        var roleDto = new RoleDto(
            updatedRole!.Id,
            updatedRole.Name,
            updatedRole.Description,
            updatedRole.IsActive,
            updatedRole.CreatedAt,
            updatedRole.UpdatedAt,
            [.. updatedRole.RolePermissions.Select(rp => new RolePermissionDto(
                rp.PermissionId,
                rp.Permission.Resource,
                rp.Permission.Action,
                rp.Permission.GetPermissionString()
            ))]
        );

        return Result.Success(roleDto);
    }
}
