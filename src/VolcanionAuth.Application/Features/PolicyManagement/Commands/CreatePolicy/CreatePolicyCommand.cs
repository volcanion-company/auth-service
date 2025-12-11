using VolcanionAuth.Application.Features.PolicyManagement.Common;

namespace VolcanionAuth.Application.Features.PolicyManagement.Commands.CreatePolicy;

/// <summary>
/// Represents a command to create a new policy with specified attributes, including name, resource, action, effect,
/// conditions, priority, and an optional description.
/// </summary>
/// <param name="Name">The unique name of the policy to be created. Cannot be null or empty.</param>
/// <param name="Resource">The resource to which the policy applies. Specifies the target of the policy rule.</param>
/// <param name="Action">The action that the policy governs, such as 'read', 'write', or 'delete'.</param>
/// <param name="Effect">The effect of the policy, typically indicating whether the action is allowed or denied. Common values are 'Allow' or
/// 'Deny'.</param>
/// <param name="Conditions">The conditions under which the policy is applicable, expressed as a string. May be empty if no conditions are
/// required.</param>
/// <param name="Priority">The priority of the policy. Policies with higher priority values are evaluated before those with lower values.
/// Defaults to 0.</param>
/// <param name="Description">An optional description providing additional details about the policy. Can be null.</param>
public record CreatePolicyCommand(
    string Name,
    string Resource,
    string Action,
    string Effect,
    string Conditions,
    int Priority = 0,
    string? Description = null
) : IRequest<Result<PolicyDto>>;
