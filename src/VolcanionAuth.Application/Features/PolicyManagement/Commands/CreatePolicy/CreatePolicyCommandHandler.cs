using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Application.Features.PolicyManagement.Common;
using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Application.Features.PolicyManagement.Commands.CreatePolicy;

/// <summary>
/// Handles the creation of new policy entities in response to a create policy command.
/// </summary>
/// <remarks>This handler ensures that policy names are unique and that the effect specified in the command is
/// valid before creating and saving a new policy. It maps the created policy to a data transfer object (DTO) for the
/// result. The operation is performed asynchronously and changes are committed atomically using the provided unit of
/// work.</remarks>
/// <param name="policyRepository">The repository used to persist new policy entities.</param>
/// <param name="readPolicyRepository">The repository used to query existing policy entities for validation purposes.</param>
/// <param name="unitOfWork">The unit of work used to commit changes to the data store as part of the policy creation process.</param>
public class CreatePolicyCommandHandler(
    IRepository<Policy> policyRepository,
    IReadRepository<Policy> readPolicyRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreatePolicyCommand, Result<PolicyDto>>
{
    /// <summary>
    /// Handles the creation of a new policy based on the specified command, validating input and ensuring uniqueness of
    /// the policy name.
    /// </summary>
    /// <remarks>The method validates that the policy name is unique and that the effect is either 'Allow' or
    /// 'Deny'. If validation fails, a failure result is returned. The operation is performed asynchronously and may be
    /// cancelled via the provided cancellation token.</remarks>
    /// <param name="request">The command containing the details of the policy to create, including name, resource, action, effect,
    /// conditions, priority, and description.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A result containing the created policy as a PolicyDto if successful; otherwise, a failure result with an error
    /// message describing the reason for failure.</returns>
    public async Task<Result<PolicyDto>> Handle(CreatePolicyCommand request, CancellationToken cancellationToken)
    {
        // Check if policy name already exists
        var allPolicies = await readPolicyRepository.GetAllAsync(cancellationToken);
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
        await policyRepository.AddAsync(policy, cancellationToken);
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
