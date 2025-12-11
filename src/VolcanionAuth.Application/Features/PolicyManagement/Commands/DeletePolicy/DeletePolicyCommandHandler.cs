using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Application.Features.PolicyManagement.Commands.DeletePolicy;

/// <summary>
/// Handles requests to delete a policy by its identifier.
/// </summary>
/// <remarks>This handler locates the specified policy and removes it from the data store. If the policy does not
/// exist, the operation fails and returns an appropriate result. The handler ensures that changes are persisted using
/// the provided unit of work.</remarks>
/// <param name="policyRepository">The repository used to perform write operations on policy entities.</param>
/// <param name="unitOfWork">The unit of work used to commit changes to the data store.</param>
public class DeletePolicyCommandHandler(
    IRepository<Policy> policyRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeletePolicyCommand, Result>
{
    /// <summary>
    /// Handles the deletion of a policy identified by the specified command.
    /// </summary>
    /// <param name="request">The command containing the ID of the policy to delete. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A result indicating whether the policy was successfully deleted. Returns a failure result if the policy does not
    /// exist.</returns>
    public async Task<Result> Handle(DeletePolicyCommand request, CancellationToken cancellationToken)
    {
        // Find the policy from WRITE repository
        var policy = await policyRepository.GetByIdAsync(request.PolicyId, cancellationToken);
        if (policy == null)
        {
            return Result.Failure($"Policy with ID '{request.PolicyId}' was not found");
        }

        // Delete the policy
        policyRepository.Remove(policy);
        // Persist the changes
        await unitOfWork.SaveChangesAsync(cancellationToken);
        // Return success
        return Result.Success();
    }
}
