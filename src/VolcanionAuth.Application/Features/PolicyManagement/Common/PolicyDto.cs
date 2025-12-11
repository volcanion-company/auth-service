namespace VolcanionAuth.Application.Features.PolicyManagement.Common;

/// <summary>
/// Represents a data transfer object for an access control policy, including its identity, target resource, permitted
/// action, effect, conditions, and metadata.
/// </summary>
/// <remarks>Use this record to transfer policy information between application layers or services. All string
/// parameters except <paramref name="Description"/> must be non-null and non-empty to ensure valid policy
/// definitions.</remarks>
/// <param name="PolicyId">The unique identifier of the policy.</param>
/// <param name="Name">The name of the policy. Cannot be null or empty.</param>
/// <param name="Description">An optional description providing additional details about the policy.</param>
/// <param name="Resource">The resource to which the policy applies. Cannot be null or empty.</param>
/// <param name="Action">The action that the policy governs (for example, 'read', 'write', or 'delete'). Cannot be null or empty.</param>
/// <param name="Effect">The effect of the policy, such as 'allow' or 'deny'. Cannot be null or empty.</param>
/// <param name="Conditions">A serialized representation of any conditions that must be met for the policy to apply. Cannot be null or empty.</param>
/// <param name="Priority">The priority of the policy. Higher values indicate higher precedence when multiple policies apply.</param>
/// <param name="IsActive">Indicates whether the policy is currently active. Set to <see langword="true"/> to enable the policy; otherwise,
/// <see langword="false"/>.</param>
/// <param name="CreatedAt">The date and time when the policy was created, in UTC.</param>
/// <param name="UpdatedAt">The date and time when the policy was last updated, in UTC, or <see langword="null"/> if the policy has not been
/// updated since creation.</param>
public record PolicyDto(
    Guid PolicyId,
    string Name,
    string? Description,
    string Resource,
    string Action,
    string Effect,
    string Conditions,
    int Priority,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
