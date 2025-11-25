using MediatR;
using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Domain.Common;

namespace VolcanionAuth.Application.Features.Authorization.Queries.EvaluatePolicy;

public class EvaluatePolicyQueryHandler : IRequestHandler<EvaluatePolicyQuery, Result<EvaluatePolicyResponse>>
{
    private readonly IAuthorizationService _authorizationService;

    public EvaluatePolicyQueryHandler(IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }

    public async Task<Result<EvaluatePolicyResponse>> Handle(EvaluatePolicyQuery request, CancellationToken cancellationToken)
    {
        // First check RBAC permissions
        var hasPermission = await _authorizationService.HasPermissionAsync(
            request.UserId,
            request.Resource,
            request.Action,
            cancellationToken);

        if (hasPermission)
        {
            return Result.Success(new EvaluatePolicyResponse(true, "Allowed by RBAC"));
        }

        // Then evaluate ABAC policies
        var policyAllowed = await _authorizationService.EvaluatePolicyAsync(
            request.UserId,
            request.Resource,
            request.Action,
            request.Context,
            cancellationToken);

        if (policyAllowed)
        {
            return Result.Success(new EvaluatePolicyResponse(true, "Allowed by policy"));
        }

        return Result.Success(new EvaluatePolicyResponse(false, "Access denied"));
    }
}
