using VolcanionAuth.Application.Features.PolicyManagement.Common;

namespace VolcanionAuth.Application.Features.PolicyManagement.Commands.TogglePolicyStatus;

/// <summary>
/// Command to activate or deactivate a policy.
/// </summary>
public record TogglePolicyStatusCommand(Guid PolicyId, bool IsActive) : IRequest<Result<PolicyDto>>;
