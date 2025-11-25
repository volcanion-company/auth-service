using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Application.Common.Interfaces;

/// <summary>
/// Hybrid authorization service (RBAC + ABAC + ReBAC)
/// </summary>
public interface IAuthorizationService
{
    Task<bool> HasPermissionAsync(Guid userId, string resource, string action, CancellationToken cancellationToken = default);
    Task<bool> EvaluatePolicyAsync(Guid userId, string resource, string action, Dictionary<string, object> context, CancellationToken cancellationToken = default);
    Task<bool> HasRelationshipAsync(Guid userId, Guid targetUserId, string relationshipType, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);
}
