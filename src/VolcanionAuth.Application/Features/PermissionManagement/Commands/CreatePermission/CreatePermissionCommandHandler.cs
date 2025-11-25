using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Application.Features.PermissionManagement.Common;
using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Application.Features.PermissionManagement.Commands.CreatePermission;

/// <summary>
/// Handler for creating a new permission.
/// </summary>
public class CreatePermissionCommandHandler : IRequestHandler<CreatePermissionCommand, Result<PermissionDto>>
{
    private readonly IRepository<Permission> _permissionRepository;
    private readonly IReadRepository<Permission> _readPermissionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePermissionCommandHandler(
        IRepository<Permission> permissionRepository,
        IReadRepository<Permission> readPermissionRepository,
        IUnitOfWork unitOfWork)
    {
        _permissionRepository = permissionRepository;
        _readPermissionRepository = readPermissionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PermissionDto>> Handle(CreatePermissionCommand request, CancellationToken cancellationToken)
    {
        // Check if permission already exists
        var allPermissions = await _readPermissionRepository.GetAllAsync(cancellationToken);
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
        await _permissionRepository.AddAsync(permission, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

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
