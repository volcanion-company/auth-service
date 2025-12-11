using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Application.Features.PermissionManagement.Commands.DeletePermission;

/// <summary>
/// Handles requests to delete a permission from the system, ensuring that the permission is not assigned to any roles
/// before removal.
/// </summary>
/// <remarks>This handler enforces that a permission cannot be deleted if it is currently assigned to one or more
/// roles. The operation is transactional and changes are committed only if the deletion is valid.</remarks>
/// <param name="permissionRepository">The repository used to remove permission entities from persistent storage.</param>
/// <param name="unitOfWork">The unit of work used to commit changes to the data store after a permission is deleted.</param>
public class DeletePermissionCommandHandler(
    IRepository<Permission> permissionRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeletePermissionCommand, Result>
{
    /// <summary>
    /// Attempts to delete the specified permission if it is not assigned to any roles.
    /// </summary>
    /// <remarks>This method will not delete a permission that is currently assigned to any roles. To delete
    /// such a permission, it must first be unassigned from all roles.</remarks>
    /// <param name="request">The command containing the ID of the permission to delete.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A result indicating whether the permission was successfully deleted. Returns a failure result if the permission
    /// does not exist or is assigned to one or more roles.</returns>
    public async Task<Result> Handle(DeletePermissionCommand request, CancellationToken cancellationToken)
    {
        // Find the permission from WRITE repository with RolePermissions loaded
        var permission = await permissionRepository.GetPermissionWithRolesAsync(request.PermissionId, cancellationToken);
        if (permission == null)
        {
            return Result.Failure($"Permission with ID '{request.PermissionId}' was not found");
        }

        // Check if permission is assigned to any roles
        if (permission.RolePermissions.Count != 0)
        {
            return Result.Failure($"Cannot delete permission '{permission.GetPermissionString()}' because it is assigned to {permission.RolePermissions.Count} role(s)");
        }

        // Delete the permission
        permissionRepository.Remove(permission);
        // Commit the changes
        await unitOfWork.SaveChangesAsync(cancellationToken);
        // Return success
        return Result.Success();
    }
}
