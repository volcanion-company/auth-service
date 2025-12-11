using VolcanionAuth.Application.Features.PolicyManagement.Common;

namespace VolcanionAuth.Application.Features.PolicyManagement.Commands.TogglePolicyStatus;

/// <summary>
/// Represents a request to update the activation status of a specific policy.
/// </summary>
/// <param name="PolicyId">The unique identifier of the policy whose status is to be updated.</param>
/// <param name="IsActive">A value indicating whether the policy should be set as active. Set to <see langword="true"/> to activate the policy;
/// otherwise, <see langword="false"/> to deactivate it.</param>
public record TogglePolicyStatusCommand(Guid PolicyId, bool IsActive) : IRequest<Result<PolicyDto>>;
