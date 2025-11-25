using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Domain.Entities;
using VolcanionAuth.Infrastructure.Persistence;

namespace VolcanionAuth.Infrastructure.Security;

/// <summary>
/// Hybrid Authorization Service (RBAC + ABAC + ReBAC)
/// </summary>
public class HybridAuthorizationService : IAuthorizationService
{
    private readonly ReadDbContext _readContext;
    private readonly ICacheService _cacheService;

    public HybridAuthorizationService(ReadDbContext readContext, ICacheService cacheService)
    {
        _readContext = readContext;
        _cacheService = cacheService;
    }

    public async Task<bool> HasPermissionAsync(Guid userId, string resource, string action, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"permission_check:{userId}:{resource}:{action}";
        var cached = await _cacheService.GetAsync<bool?>(cacheKey, cancellationToken);
        if (cached.HasValue)
            return cached.Value;

        var hasPermission = await _readContext.Set<User>()
            .Where(u => u.Id == userId)
            .SelectMany(u => u.UserRoles)
            .SelectMany(ur => ur.Role.RolePermissions)
            .AnyAsync(rp => rp.Permission.Resource == resource && rp.Permission.Action == action, cancellationToken);

        await _cacheService.SetAsync(cacheKey, hasPermission, TimeSpan.FromMinutes(10), cancellationToken);

        return hasPermission;
    }

    public async Task<bool> EvaluatePolicyAsync(
        Guid userId,
        string resource,
        string action,
        Dictionary<string, object> context,
        CancellationToken cancellationToken = default)
    {
        // Get user attributes
        var userAttributes = await _readContext.Set<UserAttribute>()
            .Where(ua => ua.UserId == userId)
            .ToDictionaryAsync(ua => ua.AttributeKey, ua => ua.AttributeValue, cancellationToken);

        // Get active policies for the resource and action
        var policies = await _readContext.Set<Policy>()
            .Where(p => p.IsActive && p.Resource == resource && p.Action == action)
            .OrderByDescending(p => p.Priority)
            .ToListAsync(cancellationToken);

        foreach (var policy in policies)
        {
            var isMatch = EvaluatePolicyConditions(policy.Conditions, userAttributes, context);
            
            if (isMatch)
            {
                return policy.Effect == "Allow";
            }
        }

        return false;
    }

    public async Task<bool> HasRelationshipAsync(
        Guid userId,
        Guid targetUserId,
        string relationshipType,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"relationship:{userId}:{targetUserId}:{relationshipType}";
        var cached = await _cacheService.GetAsync<bool?>(cacheKey, cancellationToken);
        if (cached.HasValue)
            return cached.Value;

        var hasRelationship = await _readContext.Set<UserRelationship>()
            .AnyAsync(ur =>
                ur.SourceUserId == userId &&
                ur.TargetUserId == targetUserId &&
                ur.RelationshipType == relationshipType,
                cancellationToken);

        await _cacheService.SetAsync(cacheKey, hasRelationship, TimeSpan.FromMinutes(10), cancellationToken);

        return hasRelationship;
    }

    public async Task<IReadOnlyList<string>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"user_permissions:{userId}";
        var cached = await _cacheService.GetAsync<List<string>>(cacheKey, cancellationToken);
        if (cached != null)
            return cached;

        var permissions = await _readContext.Set<User>()
            .Where(u => u.Id == userId)
            .SelectMany(u => u.UserRoles)
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Resource + ":" + rp.Permission.Action)
            .Distinct()
            .ToListAsync(cancellationToken);

        await _cacheService.SetAsync(cacheKey, permissions, TimeSpan.FromMinutes(15), cancellationToken);

        return permissions;
    }

    private bool EvaluatePolicyConditions(
        string conditionsJson,
        Dictionary<string, string> userAttributes,
        Dictionary<string, object> context)
    {
        try
        {
            var conditions = JsonSerializer.Deserialize<Dictionary<string, object>>(conditionsJson);
            if (conditions == null)
                return false;

            foreach (var condition in conditions)
            {
                var key = condition.Key;
                var expectedValue = condition.Value;

                // Check user attributes
                if (userAttributes.ContainsKey(key))
                {
                    if (!CompareValues(userAttributes[key], expectedValue))
                        return false;
                }
                // Check context
                else if (context.ContainsKey(key))
                {
                    if (!CompareValues(context[key], expectedValue))
                        return false;
                }
                else
                {
                    return false; // Required attribute not found
                }
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    private bool CompareValues(object actual, object expected)
    {
        if (actual == null || expected == null)
            return actual == expected;

        var actualStr = actual.ToString();
        var expectedStr = expected.ToString();

        return string.Equals(actualStr, expectedStr, StringComparison.OrdinalIgnoreCase);
    }
}
