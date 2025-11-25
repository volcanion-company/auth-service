namespace VolcanionAuth.Application.Features.PolicyManagement.Common;

/// <summary>
/// Data transfer object representing a policy with its details.
/// </summary>
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
