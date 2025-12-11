using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Application.Features.RoleManagement.Common;
using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Application.Features.RoleManagement.Commands.UpdateRole;

/// <summary>
/// Handles the update operation for an existing role, including its name, description, and associated permissions.
/// </summary>
/// <remarks>This handler validates the existence of the role and permissions before applying updates. It ensures
/// that role names remain unique and that only valid permission IDs are assigned. All changes are committed atomically
/// using the provided unit of work.</remarks>
/// <param name="roleRepository">The repository used to persist changes to role entities.</param>
/// <param name="readRoleRepository">The repository used to retrieve role entities for validation and data loading.</param>
/// <param name="permissionRepository">The repository used to access permission entities when updating role permissions.</param>
/// <param name="unitOfWork">The unit of work used to commit changes to the data store as part of the update operation.</param>
public class UpdateRoleCommandHandler(
    IRepository<Role> roleRepository,
    IReadRepository<Role> readRoleRepository,
    IReadRepository<Permission> permissionRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateRoleCommand, Result<RoleDto>>
{
    /// <summary>
    /// Handles the update of an existing role, including its name, description, and permissions, based on the provided
    /// command.
    /// </summary>
    /// <remarks>If the specified role does not exist, or if any provided permission ID is invalid, the
    /// operation fails and returns an appropriate error message. The method ensures that role names remain unique and
    /// that all permission IDs are valid before applying changes.</remarks>
    /// <param name="request">The command containing the role ID to update, along with the new name, description, and permission IDs to apply.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A result containing the updated role data transfer object if the update succeeds; otherwise, a failure result
    /// with an error message describing why the update could not be completed.</returns>
    public async Task<Result<RoleDto>> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        // Find the role from WRITE repository
        var role = await roleRepository.GetRoleWithPermissionsAsync(request.RoleId, cancellationToken);
        if (role == null)
        {
            return Result.Failure<RoleDto>($"Role with ID '{request.RoleId}' was not found");
        }

        // Update name if provided
        if (!string.IsNullOrWhiteSpace(request.Name) && request.Name != role.Name)
        {
            // Check if new name already exists
            var allRoles = await readRoleRepository.GetAllAsync(cancellationToken);
            if (allRoles.Any(r => r.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase) && r.Id != request.RoleId))
            {
                return Result.Failure<RoleDto>($"A role with the name '{request.Name}' already exists");
            }

            var updateResult = role.Update(request.Name, request.Description ?? role.Description);
            if (updateResult.IsFailure)
            {
                return Result.Failure<RoleDto>(updateResult.Error);
            }
        }
        else if (request.Description != null && request.Description != role.Description)
        {
            var updateResult = role.Update(role.Name, request.Description);
            if (updateResult.IsFailure)
            {
                return Result.Failure<RoleDto>(updateResult.Error);
            }
        }

        // Update permissions if provided
        if (request.PermissionIds != null)
        {
            var allPermissions = await permissionRepository.GetAllAsync(cancellationToken);
            var requestedPermissions = allPermissions.Where(p => request.PermissionIds.Contains(p.Id)).ToList();

            if (requestedPermissions.Count != request.PermissionIds.Count)
            {
                var foundIds = requestedPermissions.Select(p => p.Id).ToList();
                var missingIds = request.PermissionIds.Except(foundIds).ToList();
                return Result.Failure<RoleDto>($"The following permission IDs were not found: {string.Join(", ", missingIds)}");
            }

            // Clear existing permissions
            var existingPermissionIds = role.RolePermissions.Select(rp => rp.PermissionId).ToList();
            foreach (var permissionId in existingPermissionIds)
            {
                role.RemovePermission(permissionId);
            }

            // Add new permissions
            foreach (var permissionId in request.PermissionIds)
            {
                role.AddPermission(permissionId);
            }
        }

        // NO need to call Update - entity is already tracked
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload role with permissions for DTO mapping
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
