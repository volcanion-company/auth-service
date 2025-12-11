using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Application.Features.PermissionManagement.Commands.DeletePermission;

/// <summary>
/// Handler for deleting a permission from the system.
/// </summary>
public class DeletePermissionCommandHandler : IRequestHandler<DeletePermissionCommand, Result>
{
    private readonly IRepository<Permission> _permissionRepository;
    private readonly IReadRepository<Permission> _readPermissionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeletePermissionCommandHandler(
        IRepository<Permission> permissionRepository,
        IReadRepository<Permission> readPermissionRepository,
        IUnitOfWork unitOfWork)
    {
        _permissionRepository = permissionRepository;
        _readPermissionRepository = readPermissionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeletePermissionCommand request, CancellationToken cancellationToken)
    {
        // Find the permission
        var permission = await _readPermissionRepository.GetByIdAsync(request.PermissionId, cancellationToken);
        if (permission == null)
        {
            return Result.Failure($"Permission with ID '{request.PermissionId}' was not found");
        }

        // Check if permission is assigned to any roles
        if (permission.RolePermissions.Any())
        {
            return Result.Failure($"Cannot delete permission '{permission.GetPermissionString()}' because it is assigned to {permission.RolePermissions.Count} role(s)");
        }

        // Delete the permission
        _permissionRepository.Remove(permission);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
