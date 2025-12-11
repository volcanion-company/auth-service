namespace VolcanionAuth.Application.Features.PolicyManagement.Commands.DeletePolicy;

/// <summary>
/// Command to delete a policy from the system.
/// </summary>
public record DeletePolicyCommand(Guid PolicyId) : IRequest<Result>;
