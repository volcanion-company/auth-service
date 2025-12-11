namespace VolcanionAuth.Application.Features.PolicyManagement.Commands.DeletePolicy;

/// <summary>
/// Represents a request to delete a policy identified by its unique identifier.
/// </summary>
/// <param name="PolicyId">The unique identifier of the policy to be deleted. Must correspond to an existing policy.</param>
public record DeletePolicyCommand(Guid PolicyId) : IRequest<Result>;
