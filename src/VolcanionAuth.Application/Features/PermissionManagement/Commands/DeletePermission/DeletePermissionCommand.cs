namespace VolcanionAuth.Application.Features.PermissionManagement.Commands.DeletePermission;

/// <summary>
/// Represents a request to delete a permission identified by its unique ID.
/// </summary>
/// <param name="PermissionId">The unique identifier of the permission to be deleted.</param>
public record DeletePermissionCommand(Guid PermissionId) : IRequest<Result>;
