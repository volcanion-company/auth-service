using VolcanionAuth.Application.Features.PolicyManagement.Common;

namespace VolcanionAuth.Application.Features.PolicyManagement.Commands.UpdatePolicy;

/// <summary>
/// Command to update an existing policy's information.
/// </summary>
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
