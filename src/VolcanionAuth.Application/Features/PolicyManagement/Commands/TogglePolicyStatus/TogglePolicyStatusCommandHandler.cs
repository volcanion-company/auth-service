using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Application.Features.PolicyManagement.Common;
using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Application.Features.PolicyManagement.Commands.TogglePolicyStatus;

/// <summary>
/// Handler for activating or deactivating a policy.
/// </summary>
public class TogglePolicyStatusCommandHandler : IRequestHandler<TogglePolicyStatusCommand, Result<PolicyDto>>
{
    private readonly IRepository<Policy> _policyRepository;
    private readonly IReadRepository<Policy> _readPolicyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TogglePolicyStatusCommandHandler(
        IRepository<Policy> policyRepository,
        IReadRepository<Policy> readPolicyRepository,
        IUnitOfWork unitOfWork)
    {
        _policyRepository = policyRepository;
        _readPolicyRepository = readPolicyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PolicyDto>> Handle(TogglePolicyStatusCommand request, CancellationToken cancellationToken)
    {
        // Find the policy
        var policy = await _readPolicyRepository.GetByIdAsync(request.PolicyId, cancellationToken);
        if (policy == null)
        {
            return Result.Failure<PolicyDto>($"Policy with ID '{request.PolicyId}' was not found");
        }

        // Toggle status
        if (request.IsActive)
        {
            policy.Activate();
        }
        else
        {
            policy.Deactivate();
        }

        // Save changes
        _policyRepository.Update(policy);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

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
