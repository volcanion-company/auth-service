using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Application.Features.RoleManagement.Common;
using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Application.Features.RoleManagement.Commands.ToggleRoleStatus;

/// <summary>
/// Handles the command to toggle the active status of a role and persists the change.
/// </summary>
/// <remarks>This handler retrieves the specified role, toggles its active status based on the command, and saves
/// the changes. The updated role is returned as a data transfer object. If the role does not exist, a failure result is
/// returned.</remarks>
/// <param name="roleRepository">The repository used to update role entities in the data store.</param>
/// <param name="readRoleRepository">The repository used to retrieve role entities along with their permissions.</param>
/// <param name="unitOfWork">The unit of work used to commit changes to the data store.</param>
public class ToggleRoleStatusCommandHandler(
    IRepository<Role> roleRepository,
    IReadRepository<Role> readRoleRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<ToggleRoleStatusCommand, Result<RoleDto>>
{
    /// <summary>
    /// Toggles the active status of a role and returns the updated role information.
    /// </summary>
    /// <remarks>Returns a failure result if the specified role does not exist.</remarks>
    /// <param name="request">The command containing the role identifier and the desired active status.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A result containing the updated role data transfer object if the operation succeeds; otherwise, a failure result
    /// with an error message.</returns>
    public async Task<Result<RoleDto>> Handle(ToggleRoleStatusCommand request, CancellationToken cancellationToken)
    {
        // Find the role with permissions
        var role = await readRoleRepository.GetRoleWithPermissionsAsync(request.RoleId, cancellationToken);
        if (role == null)
        {
            return Result.Failure<RoleDto>($"Role with ID '{request.RoleId}' was not found");
        }

        // Toggle status
        if (request.IsActive)
        {
            role.Activate();
        }
        else
        {
            role.Deactivate();
        }

        // Save changes
        roleRepository.Update(role);
        await unitOfWork.SaveChangesAsync(cancellationToken);

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
