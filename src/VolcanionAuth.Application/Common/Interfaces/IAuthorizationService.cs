namespace VolcanionAuth.Application.Common.Interfaces;

/// <summary>
/// Defines a contract for evaluating user authorization, permissions, and relationships within an application.
/// </summary>
/// <remarks>Implementations of this interface provide methods to check user permissions, evaluate authorization
/// policies, and determine relationships between users. These methods are typically used to enforce access control and
/// authorization logic in applications. All methods are asynchronous and support cancellation via a
/// CancellationToken.</remarks>
public interface IAuthorizationService
{
    /// <summary>
    /// Asynchronously determines whether the specified user has permission to perform the given action on the specified
    /// resource.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose permissions are being checked.</param>
    /// <param name="resource">The name or identifier of the resource for which access is being evaluated. Cannot be null or empty.</param>
    /// <param name="action">The action to check permission for, such as "read", "write", or "delete". Cannot be null or empty.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the permission check operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the user has the
    /// specified permission; otherwise, <see langword="false"/>.</returns>
    Task<bool> HasPermissionAsync(Guid userId, string resource, string action, CancellationToken cancellationToken = default);
    /// <summary>
    /// Asynchronously evaluates whether the specified user is authorized to perform the given action on the resource
    /// according to the current policy.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose access is being evaluated.</param>
    /// <param name="resource">The name or identifier of the resource for which access is requested. Cannot be null or empty.</param>
    /// <param name="action">The action the user intends to perform on the resource. Cannot be null or empty.</param>
    /// <param name="context">A dictionary containing additional contextual information used during policy evaluation. May be null if no extra
    /// context is required.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the user is
    /// authorized to perform the action on the resource; otherwise, <see langword="false"/>.</returns>
    Task<bool> EvaluatePolicyAsync(Guid userId, string resource, string action, Dictionary<string, object> context, CancellationToken cancellationToken = default);
    /// <summary>
    /// Asynchronously determines whether a specified relationship exists between two users.
    /// </summary>
    /// <param name="userId">The unique identifier of the user for whom the relationship is being checked.</param>
    /// <param name="targetUserId">The unique identifier of the target user involved in the relationship.</param>
    /// <param name="relationshipType">The type of relationship to check for, such as "friend" or "blocked". Cannot be null or empty.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the specified
    /// relationship exists; otherwise, <see langword="false"/>.</returns>
    Task<bool> HasRelationshipAsync(Guid userId, Guid targetUserId, string relationshipType, CancellationToken cancellationToken = default);
    /// <summary>
    /// Asynchronously retrieves the list of permissions assigned to the specified user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose permissions are to be retrieved.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a read-only list of permission names
    /// assigned to the user. The list will be empty if the user has no permissions.</returns>
    Task<IReadOnlyList<string>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Asynchronously determines whether the specified user is authorized to perform the given action on the specified
    /// resource.
    /// </summary>
    /// <remarks>The method does not block the calling thread. The authorization decision may depend on the
    /// provided context and can vary based on implementation. Thread safety and performance may depend on the
    /// underlying authorization provider.</remarks>
    /// <param name="userId">The unique identifier of the user whose authorization is being evaluated.</param>
    /// <param name="resource">The name or identifier of the resource for which access is being requested. Cannot be null or empty.</param>
    /// <param name="action">The action to be performed on the resource, such as "read", "write", or "delete". Cannot be null or empty.</param>
    /// <param name="context">An optional dictionary containing additional contextual information that may influence the authorization
    /// decision. May be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the authorization operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an AuthorizationResult indicating
    /// whether the user is authorized and any relevant details.</returns>
    Task<AuthorizationResult> AuthorizeAsync(Guid userId, string resource, string action, Dictionary<string, object>? context = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents the result of an authorization check, including whether access is allowed, the reason for the decision,
/// and the type of authorization evaluated.
/// </summary>
/// <param name="IsAllowed">Indicates whether the requested action is permitted. Set to <see langword="true"/> if access is allowed; otherwise,
/// <see langword="false"/>.</param>
/// <param name="Reason">A descriptive message explaining the reason for the authorization decision. May be empty if no additional
/// information is available.</param>
/// <param name="Type">The type of authorization that was evaluated to produce this result.</param>
public record AuthorizationResult(
    bool IsAllowed,
    string Reason,
    AuthorizationType Type
);

/// <summary>
/// Specifies the type of authorization mechanism to be used when evaluating access control.
/// </summary>
/// <remarks>Use this enumeration to indicate whether authorization should be based on permissions, policies,
/// relationships, or if no authorization is required. The selected value determines how access checks are performed
/// within the system.</remarks>
public enum AuthorizationType
{
    /// <summary>
    /// Specifies that no options are set.
    /// </summary>
    None = 0,
    /// <summary>
    /// Specifies that the error is related to insufficient permissions.
    /// </summary>
    Permission = 1,
    /// <summary>
    /// Represents a policy document or item within the enumeration.
    /// </summary>
    Policy = 2,
    /// <summary>
    /// Represents a relationship entity type within the enumeration.
    /// </summary>
    Relationship = 3,
}
