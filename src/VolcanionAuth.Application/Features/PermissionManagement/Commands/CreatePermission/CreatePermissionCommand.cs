using VolcanionAuth.Application.Features.PermissionManagement.Common;

namespace VolcanionAuth.Application.Features.PermissionManagement.Commands.CreatePermission;

/// <summary>
/// Command to create a new permission in the system.
/// </summary>
public record CreatePermissionCommand(
    string Resource,
    string Action,
    string? Description = null
) : IRequest<Result<PermissionDto>>;
