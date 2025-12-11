using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Application.Features.PermissionManagement.Common;
using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Application.Features.PermissionManagement.Queries.GetPermissionById;

/// <summary>
/// Handler for retrieving detailed information about a specific permission.
/// </summary>
public class GetPermissionByIdQueryHandler : IRequestHandler<GetPermissionByIdQuery, Result<PermissionDto>>
{
    private readonly IReadRepository<Permission> _permissionRepository;

    public GetPermissionByIdQueryHandler(IReadRepository<Permission> permissionRepository)
    {
        _permissionRepository = permissionRepository;
    }

    public async Task<Result<PermissionDto>> Handle(GetPermissionByIdQuery request, CancellationToken cancellationToken)
    {
        // Find the permission by ID
        var permission = await _permissionRepository.GetByIdAsync(request.PermissionId, cancellationToken);

        if (permission == null)
        {
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

        return Result.Success(permissionDto);
    }
}
