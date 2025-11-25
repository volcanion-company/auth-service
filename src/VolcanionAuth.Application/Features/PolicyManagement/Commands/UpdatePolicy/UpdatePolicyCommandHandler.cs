using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Application.Features.PolicyManagement.Common;
using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Application.Features.PolicyManagement.Commands.UpdatePolicy;

/// <summary>
/// Handler for updating an existing policy's information.
/// </summary>
public class UpdatePolicyCommandHandler : IRequestHandler<UpdatePolicyCommand, Result<PolicyDto>>
{
    private readonly IRepository<Policy> _policyRepository;
    private readonly IReadRepository<Policy> _readPolicyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePolicyCommandHandler(
        IRepository<Policy> policyRepository,
        IReadRepository<Policy> readPolicyRepository,
        IUnitOfWork unitOfWork)
    {
        _policyRepository = policyRepository;
        _readPolicyRepository = readPolicyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PolicyDto>> Handle(UpdatePolicyCommand request, CancellationToken cancellationToken)
    {
        // Find the policy
        var policy = await _readPolicyRepository.GetByIdAsync(request.PolicyId, cancellationToken);
        if (policy == null)
        {
            return Result.Failure<PolicyDto>($"Policy with ID '{request.PolicyId}' was not found");
        }

        // Update name if provided
        if (!string.IsNullOrWhiteSpace(request.Name) && request.Name != policy.Name)
        {
            // Check if new name already exists
            var allPolicies = await _readPolicyRepository.GetAllAsync(cancellationToken);
            if (allPolicies.Any(p => p.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase) && p.Id != request.PolicyId))
            {
                return Result.Failure<PolicyDto>($"A policy with the name '{request.Name}' already exists");
            }
        }

        // Validate effect if provided
        if (!string.IsNullOrWhiteSpace(request.Effect) && 
            !request.Effect.Equals("Allow", StringComparison.OrdinalIgnoreCase) && 
            !request.Effect.Equals("Deny", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<PolicyDto>("Effect must be either 'Allow' or 'Deny'");
        }

        // Update policy details
        var updateResult = policy.Update(
            request.Name ?? policy.Name,
            request.Resource ?? policy.Resource,
            request.Action ?? policy.Action,
            request.Effect ?? policy.Effect,
            request.Conditions ?? policy.Conditions,
            request.Priority ?? policy.Priority,
            request.Description ?? policy.Description
        );

        if (updateResult.IsFailure)
        {
            return Result.Failure<PolicyDto>(updateResult.Error);
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
