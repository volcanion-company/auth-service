using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Application.Features.PolicyManagement.Common;
using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Application.Features.PolicyManagement.Commands.TogglePolicyStatus;

/// <summary>
/// Handles requests to toggle the active status of a policy and persists the change.
/// </summary>
/// <remarks>This handler updates the status of a policy by activating or deactivating it based on the request,
/// and returns the updated policy as a data transfer object. The operation is transactional and ensures that changes
/// are saved atomically.</remarks>
/// <param name="policyRepository">The repository used to update policy entities in the data store.</param>
/// <param name="readPolicyRepository">The repository used to retrieve policy entities for read operations.</param>
/// <param name="unitOfWork">The unit of work used to commit changes to the data store as part of the operation.</param>
public class TogglePolicyStatusCommandHandler(
    IRepository<Policy> policyRepository,
    IReadRepository<Policy> readPolicyRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<TogglePolicyStatusCommand, Result<PolicyDto>>
{
    /// <summary>
    /// Toggles the active status of the specified policy and returns the updated policy information.
    /// </summary>
    /// <param name="request">The command containing the policy identifier and the desired active status.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A result containing the updated policy data transfer object if the operation succeeds; otherwise, a failure
    /// result with an error message if the policy is not found.</returns>
    public async Task<Result<PolicyDto>> Handle(TogglePolicyStatusCommand request, CancellationToken cancellationToken)
    {
        // Find the policy
        var policy = await readPolicyRepository.GetByIdAsync(request.PolicyId, cancellationToken);
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
        policyRepository.Update(policy);
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
