using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Application.Features.PolicyManagement.Common;
using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Application.Features.PolicyManagement.Commands.CreatePolicy;

/// <summary>
/// Handler for creating a new policy.
/// </summary>
public class CreatePolicyCommandHandler : IRequestHandler<CreatePolicyCommand, Result<PolicyDto>>
{
    private readonly IRepository<Policy> _policyRepository;
    private readonly IReadRepository<Policy> _readPolicyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePolicyCommandHandler(
        IRepository<Policy> policyRepository,
        IReadRepository<Policy> readPolicyRepository,
        IUnitOfWork unitOfWork)
    {
        _policyRepository = policyRepository;
        _readPolicyRepository = readPolicyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PolicyDto>> Handle(CreatePolicyCommand request, CancellationToken cancellationToken)
    {
        // Check if policy name already exists
        var allPolicies = await _readPolicyRepository.GetAllAsync(cancellationToken);
        if (allPolicies.Any(p => p.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase)))
        {
            return Result.Failure<PolicyDto>($"A policy with the name '{request.Name}' already exists");
        }

        // Validate effect
        if (!request.Effect.Equals("Allow", StringComparison.OrdinalIgnoreCase) && 
            !request.Effect.Equals("Deny", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<PolicyDto>("Effect must be either 'Allow' or 'Deny'");
        }

        // Create the policy
        var policyResult = Policy.Create(
            request.Name,
            request.Resource,
            request.Action,
            request.Effect,
            request.Conditions,
            request.Priority,
            request.Description
        );

        if (policyResult.IsFailure)
        {
            return Result.Failure<PolicyDto>(policyResult.Error);
        }

        var policy = policyResult.Value;

        // Save the policy
        await _policyRepository.AddAsync(policy, cancellationToken);
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
