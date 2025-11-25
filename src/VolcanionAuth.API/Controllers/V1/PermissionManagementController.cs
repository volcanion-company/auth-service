using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VolcanionAuth.API.Filters;
using VolcanionAuth.Application.Features.PermissionManagement.Commands.CreatePermission;
using VolcanionAuth.Application.Features.PermissionManagement.Commands.DeletePermission;
using VolcanionAuth.Application.Features.PermissionManagement.Queries.GetAllPermissions;
using VolcanionAuth.Application.Features.PermissionManagement.Queries.GetPermissionById;
using VolcanionAuth.Application.Features.PermissionManagement.Queries.GetPermissionsGroupedByResource;

namespace VolcanionAuth.API.Controllers.V1;

/// <summary>
/// Provides API endpoints for managing permissions in the system.
/// All endpoints require authentication and specific permissions.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class PermissionManagementController(IMediator mediator, ILogger<PermissionManagementController> logger) : ControllerBase
{
    /// <summary>
    /// Retrieves a paginated list of all permissions grouped by resource.
    /// Permissions are organized by resource with pagination applied at the resource group level.
    /// </summary>
    [HttpGet]
    [RequirePermission("permissions:read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllPermissions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? resource = null,
        [FromQuery] string? searchTerm = null)
    {
        logger.LogDebug("Getting all permissions - Page: {Page}, PageSize: {PageSize}", page, pageSize);

        var query = new GetAllPermissionsQuery(page, pageSize, resource, searchTerm);
        var result = await mediator.Send(query);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Retrieves all permissions grouped by resource, ordered alphabetically.
    /// This endpoint is designed for client-side permission management and authorization UI.
    /// </summary>
    [HttpGet("grouped-by-resource")]
    [RequirePermission("permissions:read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPermissionsGroupedByResource()
    {
        logger.LogDebug("Getting all permissions grouped by resource");

        var query = new GetPermissionsGroupedByResourceQuery();
        var result = await mediator.Send(query);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Retrieves detailed information about a specific permission by ID.
    /// </summary>
    [HttpGet("{id}")]
    [RequirePermission("permissions:read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPermissionById(Guid id)
    {
        logger.LogDebug("Getting permission by ID: {PermissionId}", id);

        var query = new GetPermissionByIdQuery(id);
        var result = await mediator.Send(query);

        if (result.IsFailure)
        {
            return NotFound(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Creates a new permission in the system.
    /// </summary>
    [HttpPost]
    [RequirePermission("permissions:write")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreatePermission([FromBody] CreatePermissionCommand command)
    {
        logger.LogDebug("Creating permission: {Resource}:{Action}", command.Resource, command.Action);

        var result = await mediator.Send(command);

        if (result.IsFailure)
        {
            if (result.Error.Contains("already exists", StringComparison.OrdinalIgnoreCase))
            {
                return Conflict(new { error = result.Error });
            }
            return BadRequest(new { error = result.Error });
        }

        return CreatedAtAction(nameof(GetPermissionById), new { id = result.Value.PermissionId }, result.Value);
    }

    /// <summary>
    /// Permanently deletes a permission from the system.
    /// </summary>
    [HttpDelete("{id}")]
    [RequirePermission("permissions:delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeletePermission(Guid id)
    {
        logger.LogDebug("Deleting permission: {PermissionId}", id);

        var command = new DeletePermissionCommand(id);
        var result = await mediator.Send(command);

        if (result.IsFailure)
        {
            if (result.Error.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { error = result.Error });
            }
            return BadRequest(new { error = result.Error });
        }

        return NoContent();
    }
}
