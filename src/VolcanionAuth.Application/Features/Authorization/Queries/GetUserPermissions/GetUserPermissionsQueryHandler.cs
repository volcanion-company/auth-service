using MediatR;
using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Domain.Common;
using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Application.Features.Authorization.Queries.GetUserPermissions;

public class GetUserPermissionsQueryHandler : IRequestHandler<GetUserPermissionsQuery, Result<GetUserPermissionsResponse>>
{
    private readonly IReadRepository<User> _readRepository;
    private readonly ICacheService _cacheService;

    public GetUserPermissionsQueryHandler(IReadRepository<User> readRepository, ICacheService cacheService)
    {
        _readRepository = readRepository;
        _cacheService = cacheService;
    }

    public async Task<Result<GetUserPermissionsResponse>> Handle(GetUserPermissionsQuery request, CancellationToken cancellationToken)
    {
        // Try cache first
        var cacheKey = $"user_permissions:{request.UserId}";
        var cached = await _cacheService.GetAsync<GetUserPermissionsResponse>(cacheKey, cancellationToken);
        if (cached != null)
            return Result.Success(cached);

        var user = await _readRepository.GetUserWithPermissionsAsync(request.UserId, cancellationToken);
        if (user == null)
            return Result.Failure<GetUserPermissionsResponse>("User not found.");

        var permissions = await _readRepository.GetUserPermissionsAsync(request.UserId, cancellationToken);

        var response = new GetUserPermissionsResponse(
            user.Id,
            user.UserRoles.Select(ur => ur.RoleId.ToString()).ToList(),
            permissions.Select(p => new PermissionDto(
                p.Id,
                p.Resource,
                p.Action,
                p.GetPermissionString()
            )).ToList()
        );

        // Cache for 15 minutes
        await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(15), cancellationToken);

        return Result.Success(response);
    }
}
