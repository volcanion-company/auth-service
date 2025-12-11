using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Application.Features.PermissionManagement.Common;
using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Application.Features.PermissionManagement.Commands.CreatePermission;

/// <summary>
/// Handles the creation of a new permission by processing a CreatePermissionCommand request.
/// </summary>
/// <remarks>This handler ensures that duplicate permissions are not created by validating the resource and action
/// combination before adding a new permission. The operation is performed asynchronously and changes are committed
/// using the provided unit of work.</remarks>
/// <param name="permissionRepository">The repository used to add new Permission entities to the data store.</param>
/// <param name="readPermissionRepository">The repository used to query existing Permission entities for validation purposes.</param>
/// <param name="unitOfWork">The unit of work used to persist changes to the underlying data store.</param>
public class CreatePermissionCommandHandler(
    IRepository<Permission> permissionRepository,
    IReadRepository<Permission> readPermissionRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreatePermissionCommand, Result<PermissionDto>>
{
    /// <summary>
    /// Handles the creation of a new permission based on the specified command request.
    /// </summary>
    /// <remarks>If a permission with the same resource and action already exists, the operation fails and
    /// returns an error. The created permission will not have any roles assigned initially.</remarks>
    /// <param name="request">The command containing the details of the permission to create, including resource, action, and description.
    /// Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A result containing the created permission as a PermissionDto if successful; otherwise, a failure result with an
    /// error message.</returns>
    public async Task<Result<PermissionDto>> Handle(CreatePermissionCommand request, CancellationToken cancellationToken)
    {
        // Check if permission already exists
        var allPermissions = await readPermissionRepository.GetAllAsync(cancellationToken);
        if (allPermissions.Any(p => 
            p.Resource.Equals(request.Resource, StringComparison.OrdinalIgnoreCase) && 
            p.Action.Equals(request.Action, StringComparison.OrdinalIgnoreCase)))
        {
            return Result.Failure<PermissionDto>($"A permission with resource '{request.Resource}' and action '{request.Action}' already exists");
        }

        // Create the permission
        var permissionResult = Permission.Create(request.Resource, request.Action, request.Description);
        if (permissionResult.IsFailure)
        {
            return Result.Failure<PermissionDto>(permissionResult.Error);
        }

        var permission = permissionResult.Value;

        // Save the permission
        await permissionRepository.AddAsync(permission, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Map to DTO
        var permissionDto = new PermissionDto(
            permission.Id,
            permission.Resource,
            permission.Action,
            permission.GetPermissionString(),
            permission.Description,
            permission.CreatedAt,
            0 // No roles assigned yet
        );

        return Result.Success(permissionDto);
    }
}
