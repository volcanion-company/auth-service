namespace VolcanionAuth.Application.Features.PermissionManagement.Queries.GetPermissionsGroupedByResource;

/// <summary>
/// Represents a query to retrieve permissions grouped by resource.
/// </summary>
/// <remarks>Use this query to obtain a list of permissions, organized by their associated resources. The result
/// contains one entry per resource, each with its corresponding permissions. This query does not modify any data and is
/// typically used for read-only access in permission management scenarios.</remarks>
public record GetPermissionsGroupedByResourceQuery : IRequest<Result<List<PermissionsByResourceDto>>>;

/// <summary>
/// Represents a collection of permissions associated with a specific resource.
/// </summary>
/// <param name="Resource">The name or identifier of the resource for which permissions are defined. Cannot be null.</param>
/// <param name="Permissions">The list of permissions granted for the specified resource. Cannot be null.</param>
public record PermissionsByResourceDto(string Resource, List<PermissionItemDto> Permissions);

/// <summary>
/// Represents a permission item that defines an action, its associated permission string, and metadata for access
/// control scenarios.
/// </summary>
/// <param name="Id">The unique identifier for the permission item.</param>
/// <param name="Action">The name of the action that the permission item represents. This typically corresponds to an operation or feature
/// requiring authorization.</param>
/// <param name="Description">An optional description providing additional details about the permission item. Can be null if no description is
/// specified.</param>
/// <param name="PermissionString">The string value used to check or assign the permission for the specified action. This is typically used in
/// authorization logic.</param>
/// <param name="CreatedAt">The date and time when the permission item was created, in UTC.</param>
public record PermissionItemDto(
    Guid Id,
    string Action,
    string? Description,
    string PermissionString,
    DateTime CreatedAt
);
