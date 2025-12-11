using VolcanionAuth.Application.Features.PolicyManagement.Common;

namespace VolcanionAuth.Application.Features.PolicyManagement.Commands.UpdatePolicy;

/// <summary>
/// Represents a request to update an existing policy with new values for one or more fields.
/// </summary>
/// <remarks>Only the fields provided with non-null values will be updated. Fields left as <see langword="null"/>
/// will retain their existing values in the policy.</remarks>
/// <param name="PolicyId">The unique identifier of the policy to update.</param>
/// <param name="Name">The new name for the policy, or <see langword="null"/> to leave unchanged.</param>
/// <param name="Description">The new description for the policy, or <see langword="null"/> to leave unchanged.</param>
/// <param name="Resource">The resource to which the policy applies, or <see langword="null"/> to leave unchanged.</param>
/// <param name="Action">The action governed by the policy, or <see langword="null"/> to leave unchanged.</param>
/// <param name="Effect">The effect of the policy, such as "Allow" or "Deny"; or <see langword="null"/> to leave unchanged.</param>
/// <param name="Conditions">The conditions under which the policy applies, or <see langword="null"/> to leave unchanged.</param>
/// <param name="Priority">The priority of the policy, which determines its evaluation order; or <see langword="null"/> to leave unchanged.</param>
public record UpdatePolicyCommand(
    Guid PolicyId,
    string? Name = null,
    string? Description = null,
    string? Resource = null,
    string? Action = null,
    string? Effect = null,
    string? Conditions = null,
    int? Priority = null
) : IRequest<Result<PolicyDto>>;
