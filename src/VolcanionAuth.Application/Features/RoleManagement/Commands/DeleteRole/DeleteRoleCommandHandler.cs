using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Application.Features.RoleManagement.Commands.DeleteRole;

/// <summary>
/// Handles the command to delete a role from the system, ensuring that the role exists and is not assigned to any users
/// before removal.
/// </summary>
/// <remarks>This handler prevents deletion of roles that are currently assigned to users. If the specified role
/// does not exist or is assigned to one or more users, the operation fails and returns an appropriate error
/// message.</remarks>
/// <param name="roleRepository">The repository used to perform write operations on role entities, such as removing a role.</param>
/// <param name="readRoleRepository">The repository used to perform read operations on role entities, such as retrieving a role by its identifier.</param>
/// <param name="unitOfWork">The unit of work used to commit changes to the data store after the role is deleted.</param>
public class DeleteRoleCommandHandler(
    IRepository<Role> roleRepository,
    IReadRepository<Role> readRoleRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteRoleCommand, Result>
{
    /// <summary>
    /// Handles a request to delete a role, ensuring the role exists and is not assigned to any users before removal.
    /// </summary>
    /// <remarks>If the role is assigned to one or more users, the operation will not delete the role and will
    /// return a failure result. The method performs the deletion asynchronously and supports cancellation via the
    /// provided token.</remarks>
    /// <param name="request">The command containing the ID of the role to delete. Must specify a valid role identifier.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A result indicating whether the role was successfully deleted. Returns a failure result if the role does not
    /// exist or is assigned to users.</returns>
    public async Task<Result> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        // Find the role
        var role = await readRoleRepository.GetByIdAsync(request.RoleId, cancellationToken);
        if (role == null)
        {
            return Result.Failure($"Role with ID '{request.RoleId}' was not found");
        }

        // Check if role has assigned users
        if (role.UserRoles.Count != 0)
        {
            return Result.Failure($"Cannot delete role '{role.Name}' because it is assigned to {role.UserRoles.Count} user(s)");
        }

        // Delete the role
        roleRepository.Remove(role);
        // Save changes
        await unitOfWork.SaveChangesAsync(cancellationToken);
        // Return success
        return Result.Success();
    }
}
