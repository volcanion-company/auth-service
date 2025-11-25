using VolcanionAuth.Application.Features.PolicyManagement.Common;

namespace VolcanionAuth.Application.Features.PolicyManagement.Commands.CreatePolicy;

/// <summary>
/// Command to create a new policy in the system.
/// </summary>
public record CreatePolicyCommand(
    string Name,
    string Resource,
    string Action,
    string Effect,
    string Conditions,
    int Priority = 0,
    string? Description = null
) : IRequest<Result<PolicyDto>>;
