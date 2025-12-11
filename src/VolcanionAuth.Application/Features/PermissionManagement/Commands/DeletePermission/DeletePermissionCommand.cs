namespace VolcanionAuth.Application.Features.PermissionManagement.Commands.DeletePermission;

/// <summary>
/// Command to delete a permission from the system.
/// </summary>
public record DeletePermissionCommand(Guid PermissionId) : IRequest<Result>;
