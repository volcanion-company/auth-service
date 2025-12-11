using VolcanionAuth.Application.Features.PermissionManagement.Common;

namespace VolcanionAuth.Application.Features.PermissionManagement.Commands.CreatePermission;

/// <summary>
/// Represents a request to create a new permission for a specified resource and action.
/// </summary>
/// <param name="Resource">The name of the resource for which the permission is being created. Cannot be null or empty.</param>
/// <param name="Action">The action that the permission allows on the specified resource. Cannot be null or empty.</param>
/// <param name="Description">An optional description of the permission. Can be null if no description is required.</param>
public record CreatePermissionCommand(
    string Resource,
    string Action,
    string? Description = null
) : IRequest<Result<PermissionDto>>;
