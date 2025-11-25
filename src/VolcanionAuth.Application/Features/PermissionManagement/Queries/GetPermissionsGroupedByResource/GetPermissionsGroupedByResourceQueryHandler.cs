using MediatR;
using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Domain.Common;
using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Application.Features.PermissionManagement.Queries.GetPermissionsGroupedByResource;

public class GetPermissionsGroupedByResourceQueryHandler : IRequestHandler<GetPermissionsGroupedByResourceQuery, Result<List<PermissionsByResourceDto>>>
{
    private readonly IReadRepository<Permission> _permissionRepository;

    public GetPermissionsGroupedByResourceQueryHandler(IReadRepository<Permission> permissionRepository)
    {
        _permissionRepository = permissionRepository;
    }

    public async Task<Result<List<PermissionsByResourceDto>>> Handle(GetPermissionsGroupedByResourceQuery request, CancellationToken cancellationToken)
    {
        var allPermissions = await _permissionRepository.GetAllAsync(cancellationToken);

        var groupedPermissions = allPermissions
            .GroupBy(p => p.Resource)
            .OrderBy(g => g.Key)
            .Select(g => new PermissionsByResourceDto(
                g.Key,
                g.OrderBy(p => p.Action)
                    .Select(p => new PermissionItemDto(
                        p.Id,
                        p.Action,
                        p.Description,
                        p.GetPermissionString(),
                        p.CreatedAt
                    ))
                    .ToList()
            ))
            .ToList();

        return Result.Success(groupedPermissions);
    }
}
