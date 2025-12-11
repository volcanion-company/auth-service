using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Application.Features.PolicyManagement.Common;
using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Application.Features.PolicyManagement.Queries.GetPolicyById;

/// <summary>
/// Handler for retrieving detailed information about a specific policy.
/// </summary>
public class GetPolicyByIdQueryHandler : IRequestHandler<GetPolicyByIdQuery, Result<PolicyDto>>
{
    private readonly IReadRepository<Policy> _policyRepository;

    public GetPolicyByIdQueryHandler(IReadRepository<Policy> policyRepository)
    {
        _policyRepository = policyRepository;
    }

    public async Task<Result<PolicyDto>> Handle(GetPolicyByIdQuery request, CancellationToken cancellationToken)
    {
        // Find the policy by ID
        var policy = await _policyRepository.GetByIdAsync(request.PolicyId, cancellationToken);

        if (policy == null)
        {
            return Result.Failure<PolicyDto>($"Policy with ID '{request.PolicyId}' was not found");
        }

        // Map to DTO
        var policyDto = new PolicyDto(
            policy.Id,
            policy.Name,
            policy.Description,
            policy.Resource,
            policy.Action,
            policy.Effect,
            policy.Conditions,
            policy.Priority,
            policy.IsActive,
            policy.CreatedAt,
            policy.UpdatedAt
        );

        return Result.Success(policyDto);
    }
}
