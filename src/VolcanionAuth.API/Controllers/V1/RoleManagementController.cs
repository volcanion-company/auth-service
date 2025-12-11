using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VolcanionAuth.API.Filters;
using VolcanionAuth.Application.Features.RoleManagement.Commands.CreateRole;
using VolcanionAuth.Application.Features.RoleManagement.Commands.DeleteRole;
using VolcanionAuth.Application.Features.RoleManagement.Commands.ToggleRoleStatus;
using VolcanionAuth.Application.Features.RoleManagement.Commands.UpdateRole;
using VolcanionAuth.Application.Features.RoleManagement.Queries.GetAllRoles;
using VolcanionAuth.Application.Features.RoleManagement.Queries.GetRoleById;

namespace VolcanionAuth.API.Controllers.V1;

/// <summary>
/// Provides API endpoints for managing roles in the system.
/// All endpoints require authentication and specific permissions.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class RoleManagementController(IMediator mediator, ILogger<RoleManagementController> logger) : ControllerBase
{
    /// <summary>
    /// Retrieves a paginated list of all roles in the system.
    /// </summary>
    [HttpGet]
    [RequirePermission("roles:read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllRoles(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool includeInactive = false,
        [FromQuery] string? searchTerm = null)
    {
        logger.LogDebug("Getting all roles - Page: {Page}, PageSize: {PageSize}", page, pageSize);

        var query = new GetAllRolesQuery(page, pageSize, includeInactive, searchTerm);
        var result = await mediator.Send(query);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Retrieves detailed information about a specific role by ID.
    /// </summary>
    [HttpGet("{id}")]
    [RequirePermission("roles:read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetRoleById(Guid id)
    {
        logger.LogDebug("Getting role by ID: {RoleId}", id);

        var query = new GetRoleByIdQuery(id);
        var result = await mediator.Send(query);

        if (result.IsFailure)
        {
            return NotFound(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Creates a new role in the system.
    /// </summary>
    [HttpPost]
    [RequirePermission("roles:write")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleCommand command)
    {
        logger.LogDebug("Creating role: {Name}", command.Name);

        var result = await mediator.Send(command);

        if (result.IsFailure)
        {
            if (result.Error.Contains("already exists", StringComparison.OrdinalIgnoreCase))
            {
                return Conflict(new { error = result.Error });
            }
            return BadRequest(new { error = result.Error });
        }

        return CreatedAtAction(nameof(GetRoleById), new { id = result.Value.RoleId }, result.Value);
    }

    /// <summary>
    /// Updates an existing role's information.
    /// </summary>
    [HttpPut("{id}")]
    [RequirePermission("roles:write")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateRoleCommand command)
    {
        logger.LogDebug("Updating role: {RoleId}", id);

        if (id != command.RoleId)
        {
            return BadRequest(new { error = "Role ID in route does not match command" });
        }

        var result = await mediator.Send(command);

        if (result.IsFailure)
        {
            if (result.Error.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { error = result.Error });
            }
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Permanently deletes a role from the system.
    /// </summary>
    [HttpDelete("{id}")]
    [RequirePermission("roles:delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteRole(Guid id)
    {
        logger.LogDebug("Deleting role: {RoleId}", id);

        var command = new DeleteRoleCommand(id);
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

    /// <summary>
    /// Activates or deactivates a role.
    /// </summary>
    [HttpPatch("{id}/status")]
    [RequirePermission("roles:manage")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ToggleRoleStatus(Guid id, [FromBody] ToggleRoleStatusRequest request)
    {
        logger.LogDebug("Toggling role status: {RoleId}, IsActive: {IsActive}", id, request.IsActive);

        var command = new ToggleRoleStatusCommand(id, request.IsActive);
        var result = await mediator.Send(command);

        if (result.IsFailure)
        {
            return NotFound(new { error = result.Error });
        }

        return Ok(result.Value);
    }
}

public record ToggleRoleStatusRequest(bool IsActive);
