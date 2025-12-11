using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Application.Features.PolicyManagement.Common;
using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Application.Features.PolicyManagement.Queries.GetPolicyById;

/// <summary>
/// Handles queries to retrieve a policy by its unique identifier and returns the result as a data transfer object
/// (DTO).
/// </summary>
/// <remarks>This handler is typically used in request-response patterns to fetch policy details for display or
/// further processing. The returned result indicates success or failure, with an appropriate message if the policy is
/// not found.</remarks>
/// <param name="policyRepository">The repository used to access and retrieve policy entities from the data store.</param>
public class GetPolicyByIdQueryHandler(IReadRepository<Policy> policyRepository) : IRequestHandler<GetPolicyByIdQuery, Result<PolicyDto>>
{
    /// <summary>
    /// Retrieves a policy by its unique identifier and returns the corresponding policy data transfer object.
    /// </summary>
    /// <param name="request">The query containing the unique identifier of the policy to retrieve.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A result containing the policy data transfer object if found; otherwise, a failure result indicating that the
    /// policy was not found.</returns>
    public async Task<Result<PolicyDto>> Handle(GetPolicyByIdQuery request, CancellationToken cancellationToken)
    {
        // Find the policy by ID
        var policy = await policyRepository.GetByIdAsync(request.PolicyId, cancellationToken);

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
