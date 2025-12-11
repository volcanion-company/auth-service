using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Domain.Entities;
using VolcanionAuth.Infrastructure.Persistence;

namespace VolcanionAuth.Infrastructure.Services;

/// <summary>
/// Provides authorization services for evaluating user permissions, policies, and relationships within the application.
/// </summary>
/// <remarks>This service supports both role-based access control (RBAC) and policy-based authorization. It
/// leverages distributed caching to optimize repeated permission and policy checks. Thread safety is ensured for
/// concurrent authorization requests. Use this service to centralize and standardize authorization logic across the
/// application.</remarks>
/// <param name="context">The database context used for accessing user, role, and policy data required for authorization checks.</param>
/// <param name="policyEngine">The policy engine service used to evaluate dynamic authorization policies.</param>
/// <param name="cache">The distributed cache used to store and retrieve authorization results for improved performance.</param>
/// <param name="logger">The logger used to record authorization operations and diagnostic information.</param>
public class AuthorizationService(
    WriteDbContext context,
    IPolicyEngineService policyEngine,
    IDistributedCache cache,
    ILogger<AuthorizationService> logger) : IAuthorizationService
{
    /// <summary>
    /// Write database context for accessing authorization data.
    /// </summary>
    private readonly WriteDbContext _context = context;
    /// <summary>
    /// Cache duration in minutes for storing permission and policy evaluation results.
    /// </summary>
    private const int CacheDurationMinutes = 15;

    /// <summary>
    /// Asynchronously determines whether the specified user has permission to perform a given action on a resource
    /// based on their assigned roles.
    /// </summary>
    /// <remarks>This method uses distributed caching to improve performance for repeated permission checks.
    /// Cached results are automatically invalidated after a configured duration. Permission evaluation is based on the
    /// user's active roles and their associated permissions.</remarks>
    /// <param name="userId">The unique identifier of the user whose permissions are being checked.</param>
    /// <param name="resource">The name of the resource for which access is being evaluated. Cannot be null or empty.</param>
    /// <param name="action">The action to check permission for on the specified resource. Cannot be null or empty.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains <see langword="true"/> if the user
    /// has the specified permission; otherwise, <see langword="false"/>.</returns>
    public async Task<bool> HasPermissionAsync(Guid userId, string resource, string action, CancellationToken cancellationToken = default)
    {
        // Check cache first
        var cacheKey = $"permission:{userId}:{resource}:{action}";
        // Try to get from cache
        var cached = await cache.GetStringAsync(cacheKey, cancellationToken);
        if (!string.IsNullOrEmpty(cached))
        {
            // Log cache hit
            logger.LogDebug("Permission check cache hit for user {UserId}", userId);
            // Return cached result
            return bool.Parse(cached);
        }

        // Query database for permission
        var hasPermission = await _context.Set<User>()
            .Where(u => u.Id == userId)
            .SelectMany(u => u.UserRoles)
            .Where(ur => ur.Role.IsActive)
            .SelectMany(ur => ur.Role.RolePermissions)
            .AnyAsync(rp => rp.Permission.Resource == resource && rp.Permission.Action == action, cancellationToken);

        // Store result in cache
        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheDurationMinutes), // Cache duration
        };

        // Save to cache
        await cache.SetStringAsync(cacheKey, hasPermission.ToString(), cacheOptions, cancellationToken);
        // Log the permission check result
        logger.LogInformation("RBAC permission check: User {UserId}, Resource: {Resource}, Action: {Action}, Result: {Result}", userId, resource, action, hasPermission);
        // Return the permission result
        return hasPermission;
    }

    /// <summary>
    /// Asynchronously evaluates whether the specified user is authorized to perform a given action on a resource based
    /// on active policies.
    /// </summary>
    /// <remarks>The method evaluates all active policies that match the specified resource and action, using
    /// the provided context. If no matching policies are found, the method returns <see langword="false"/>. The context
    /// dictionary is modified by this method to include the user ID and the current UTC time.</remarks>
    /// <param name="userId">The unique identifier of the user whose access is being evaluated.</param>
    /// <param name="resource">The name of the resource for which access is being requested. Cannot be null or empty.</param>
    /// <param name="action">The action the user intends to perform on the resource. Cannot be null or empty.</param>
    /// <param name="context">A dictionary containing additional contextual information for policy evaluation. Must not be null. The method
    /// adds or updates entries for "userId" and "currentTime".</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the user is
    /// authorized to perform the action on the resource; otherwise, <see langword="false"/>.</returns>
    public async Task<bool> EvaluatePolicyAsync(
        Guid userId, 
        string resource, 
        string action, 
        Dictionary<string, object> context, 
        CancellationToken cancellationToken = default)
    {
        // Add userId to context
        context["userId"] = userId;
        context["currentTime"] = DateTime.UtcNow;

        // Find matching policies
        var policies = await _context.Set<Policy>()
            .Where(p => p.IsActive && p.Resource == resource && p.Action == action)
            .OrderByDescending(p => p.Priority)
            .ToListAsync(cancellationToken);

        // If no policies found, deny by default
        if (policies.Count == 0)
        {
            // Log no policies found
            logger.LogDebug("No policies found for resource {Resource} and action {Action}", resource, action);
            // Deny access
            return false;
        }

        // Evaluate policies using the policy engine
        var result = await policyEngine.EvaluatePoliciesAsync(policies, context, cancellationToken);
        // Log the policy evaluation result
        logger.LogInformation("Policy evaluation: User {UserId}, Resource: {Resource}, Action: {Action}, Result: {Result}, Reason: {Reason}", userId, resource, action, result.IsAllowed, result.Reason);
        // Return the evaluation result
        return result.IsAllowed;
    }

    /// <summary>
    /// Determines asynchronously whether a specified relationship exists between two users.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose relationships are being queried.</param>
    /// <param name="targetUserId">The unique identifier of the target user to check the relationship against.</param>
    /// <param name="relationshipType">The type of relationship to check for. This value is case-sensitive and must correspond to a supported
    /// relationship type.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains <see langword="true"/> if the
    /// specified relationship exists; otherwise, <see langword="false"/>.</returns>
    public async Task<bool> HasRelationshipAsync(Guid userId, Guid targetUserId, string relationshipType, CancellationToken cancellationToken = default)
    {
        // TODO: Implement ReBAC (Relationship-Based Access Control)
        // For now, return false
        await Task.CompletedTask;
        return false;
    }

    /// <summary>
    /// Asynchronously retrieves the set of permissions assigned to the specified user.
    /// </summary>
    /// <remarks>This method may return cached results to improve performance. Permissions reflect the user's
    /// active roles at the time of retrieval.</remarks>
    /// <param name="userId">The unique identifier of the user whose permissions are to be retrieved.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A read-only list of permission strings assigned to the user. Each string represents a permission in the format
    /// "Resource:Action". The list is empty if the user has no permissions.</returns>
    public async Task<IReadOnlyList<string>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Check cache first
        var cacheKey = $"user_permissions:{userId}";
        // Try to get from cache
        var cached = await cache.GetStringAsync(cacheKey, cancellationToken);
        if (!string.IsNullOrEmpty(cached))
        {
            // Return cached permissions
            return JsonSerializer.Deserialize<List<string>>(cached) ?? [];
        }

        // Query database for permissions
        var permissions = await _context.Set<User>()
            .Where(u => u.Id == userId)
            .SelectMany(u => u.UserRoles)
            .Where(ur => ur.Role.IsActive)
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => $"{rp.Permission.Resource}:{rp.Permission.Action}")
            .Distinct()
            .ToListAsync(cancellationToken);

        // Store permissions in cache
        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheDurationMinutes), // Cache duration
        };
        // Save to cache
        await cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(permissions), cacheOptions, cancellationToken);
        // Return the permissions
        return permissions;
    }

    /// <summary>
    /// Asynchronously determines whether the specified user is authorized to perform a given action on a resource,
    /// based on policies and role-based access control (RBAC) permissions.
    /// </summary>
    /// <remarks>The method first evaluates any applicable policies using the provided context. If no policy
    /// allows or explicitly denies access, it falls back to checking RBAC permissions. If neither policies nor
    /// permissions grant access, the request is denied. The evaluation is performed asynchronously and supports
    /// cancellation via the provided token.</remarks>
    /// <param name="userId">The unique identifier of the user whose authorization is being evaluated.</param>
    /// <param name="resource">The name or identifier of the resource for which access is being requested. Cannot be null.</param>
    /// <param name="action">The action the user intends to perform on the resource. Cannot be null.</param>
    /// <param name="context">An optional dictionary containing additional contextual information for policy evaluation. May be null if no
    /// context is required.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the authorization operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an AuthorizationResult indicating
    /// whether access is allowed or denied, and the reason for the decision.</returns>
    public async Task<AuthorizationResult> AuthorizeAsync(
        Guid userId,
        string resource,
        string action,
        Dictionary<string, object>? context = null,
        CancellationToken cancellationToken = default)
    {
        // 1. Check policies first (if context provided)
        if (context != null && context.Count != 0)
        {
            // Evaluate policies
            var policyResult = await EvaluatePolicyAsync(userId, resource, action, context, cancellationToken);
            if (policyResult)
            {
                // Allowed by policy
                return new AuthorizationResult(true, "Allowed by policy", AuthorizationType.Policy);
            }

            // Check if there's an explicit DENY policy
            var denyPolicies = await _context.Set<Policy>()
                .Where(p => p.IsActive && p.Resource == resource && p.Action == action && p.Effect == "Deny")
                .ToListAsync(cancellationToken);

            // If there are deny policies, evaluate them
            if (denyPolicies.Count != 0)
            {
                // Evaluate deny policies
                var denyResult = await policyEngine.EvaluatePoliciesAsync(denyPolicies, context, cancellationToken);
                if (denyResult.MatchedPolicy != null)
                {
                    // Denied by policy
                    return new AuthorizationResult(false, $"Denied by policy: {denyResult.MatchedPolicy.Name}", AuthorizationType.Policy);
                }
            }
        }

        // 2. Fallback to RBAC permission check
        var hasPermission = await HasPermissionAsync(userId, resource, action, cancellationToken);
        if (hasPermission)
        {
            // Allowed by permission
            return new AuthorizationResult(true, "Allowed by RBAC permission", AuthorizationType.Permission);
        }

        // 3. Both failed - deny access
        return new AuthorizationResult(false, "Access denied - no matching policy or permission", AuthorizationType.None);
    }
}
