using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Application.Features.PolicyManagement.Common;
using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Application.Features.PolicyManagement.Commands.UpdatePolicy;

/// <summary>
/// Handles update commands for policies, validating input and applying changes to the policy repository.
/// </summary>
/// <remarks>This handler ensures that policy updates are validated for uniqueness and correctness before
/// persisting changes. It checks for duplicate policy names and validates the effect value. The handler maps the
/// updated policy to a data transfer object (DTO) upon successful completion.</remarks>
/// <param name="policyRepository">The repository used to persist updated policy entities.</param>
/// <param name="readPolicyRepository">The read-only repository used to retrieve existing policy data for validation and lookup.</param>
/// <param name="unitOfWork">The unit of work used to commit changes to the underlying data store.</param>
public class UpdatePolicyCommandHandler(
    IRepository<Policy> policyRepository,
    IReadRepository<Policy> readPolicyRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdatePolicyCommand, Result<PolicyDto>>
{
    /// <summary>
    /// Handles an update request for an existing policy, applying changes and returning the updated policy details.
    /// </summary>
    /// <remarks>The method validates that the policy exists, ensures the new name is unique, and checks that
    /// the effect is either 'Allow' or 'Deny'. Only fields provided in the request are updated. Returns a failure
    /// result if validation fails or the policy cannot be found.</remarks>
    /// <param name="request">The update command containing the policy identifier and the new values to apply. Only non-null fields will be
    /// updated.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A result containing the updated policy details if the update succeeds; otherwise, a failure result with an error
    /// message.</returns>
    public async Task<Result<PolicyDto>> Handle(UpdatePolicyCommand request, CancellationToken cancellationToken)
    {
        // Find the policy from WRITE repository
        var policy = await policyRepository.GetByIdAsync(request.PolicyId, cancellationToken);
        if (policy == null)
        {
            return Result.Failure<PolicyDto>($"Policy with ID '{request.PolicyId}' was not found");
        }

        // Update name if provided
        if (!string.IsNullOrWhiteSpace(request.Name) && request.Name != policy.Name)
        {
            // Check if new name already exists
            var allPolicies = await readPolicyRepository.GetAllAsync(cancellationToken);
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

        // NO need to call Update - entity is already tracked
        await unitOfWork.SaveChangesAsync(cancellationToken);

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
