using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VolcanionAuth.API.Filters;
using VolcanionAuth.Application.Features.RoleManagement.Commands.CreateRole;
using VolcanionAuth.Application.Features.RoleManagement.Commands.DeleteRole;
using VolcanionAuth.Application.Features.RoleManagement.Commands.GrantPermissions;
using VolcanionAuth.Application.Features.RoleManagement.Commands.ToggleRoleStatus;
using VolcanionAuth.Application.Features.RoleManagement.Commands.UpdateRole;
using VolcanionAuth.Application.Features.RoleManagement.Queries.GetAllRoles;
using VolcanionAuth.Application.Features.RoleManagement.Queries.GetRoleById;

namespace VolcanionAuth.API.Controllers.V1;

/// <summary>
/// Provides API endpoints for managing user roles, including retrieval, creation, updating, deletion, and status
/// toggling operations. Requires authentication and appropriate permissions for each action.
/// </summary>
/// <remarks>All endpoints require the caller to be authenticated and possess the necessary permissions. Responses
/// include appropriate HTTP status codes for success and error conditions, such as 200 OK, 201 Created, 204 No Content,
/// 400 Bad Request, 403 Forbidden, 404 Not Found, and 409 Conflict. This controller is versioned as part of the API and
/// is intended for administrative use within the application.</remarks>
/// <param name="mediator">The mediator used to dispatch role management commands and queries.</param>
/// <param name="logger">The logger used to record diagnostic and operational information for role management actions.</param>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class RoleManagementController(IMediator mediator, ILogger<RoleManagementController> logger) : ControllerBase
{
    /// <summary>
    /// Retrieves a paginated list of roles, optionally including inactive roles and filtering by a search term.
    /// </summary>
    /// <remarks>Requires the 'roles:read' permission. Returns HTTP 200 (OK) with the list of roles, HTTP 400
    /// (Bad Request) if the request parameters are invalid, or HTTP 403 (Forbidden) if the user does not have
    /// sufficient permissions.</remarks>
    /// <param name="page">The page number of results to retrieve. Must be greater than zero.</param>
    /// <param name="pageSize">The number of roles to include per page. Must be greater than zero.</param>
    /// <param name="includeInactive">Specifies whether to include inactive roles in the results. Set to <see langword="true"/> to include inactive
    /// roles; otherwise, only active roles are returned.</param>
    /// <param name="searchTerm">An optional search term used to filter roles by name or description. If <see langword="null"/> or empty, no
    /// filtering is applied.</param>
    /// <returns>An <see cref="IActionResult"/> containing a paginated list of roles if successful; otherwise, a Bad Request or
    /// Forbidden response if the request is invalid or the user lacks permission.</returns>
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
        // Log the incoming request parameters for debugging purposes
        logger.LogDebug("Getting all roles - Page: {Page}, PageSize: {PageSize}", page, pageSize);
        // Create the query object with the provided parameters
        var query = new GetAllRolesQuery(page, pageSize, includeInactive, searchTerm);

        // Send the query to the mediator and await the result
        var result = await mediator.Send(query);
        if (result.IsFailure)
        {
            // Return a Bad Request response if the query failed
            return BadRequest(new { error = result.Error });
        }
        // Return the successful result
        return Ok(result.Value);
    }

    /// <summary>
    /// Retrieves the details of a role identified by the specified unique identifier.
    /// </summary>
    /// <remarks>Requires the 'roles:read' permission. Returns status code 200 with the role details on
    /// success, 404 if the role is not found, and 403 if access is denied.</remarks>
    /// <param name="id">The unique identifier of the role to retrieve.</param>
    /// <returns>An <see cref="IActionResult"/> containing the role details if found; otherwise, a response with status code 404
    /// if the role does not exist, or 403 if the caller lacks permission.</returns>
    [HttpGet("{id}")]
    [RequirePermission("roles:read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetRoleById(Guid id)
    {
        // Log the incoming request for debugging purposes
        logger.LogDebug("Getting role by ID: {RoleId}", id);
        // Create the query object with the specified role ID
        var query = new GetRoleByIdQuery(id);

        // Send the query to the mediator and await the result
        var result = await mediator.Send(query);
        if (result.IsFailure)
        {
            // Return a Not Found response if the role does not exist
            return NotFound(new { error = result.Error });
        }
        // Return the successful result
        return Ok(result.Value);
    }

    /// <summary>
    /// Creates a new role using the specified command data.
    /// </summary>
    /// <remarks>Requires the 'roles:write' permission. Returns a 409 Conflict response if a role with the
    /// specified name already exists.</remarks>
    /// <param name="command">The command containing the details of the role to create. Must not be null.</param>
    /// <returns>A 201 Created response containing the newly created role if successful; otherwise, a 400 Bad Request, 403
    /// Forbidden, or 409 Conflict response depending on the error condition.</returns>
    [HttpPost]
    [RequirePermission("roles:write")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleCommand command)
    {
        // Log the incoming request for debugging purposes
        logger.LogDebug("Creating role: {Name}", command.Name);

        // Send the command to the mediator and await the result
        var result = await mediator.Send(command);
        if (result.IsFailure)
        {
            // Check if the error indicates a conflict (e.g., role already exists)
            if (result.Error.Contains("already exists", StringComparison.OrdinalIgnoreCase))
            {
                return Conflict(new { error = result.Error });
            }
            // Return a Bad Request response for other errors
            return BadRequest(new { error = result.Error });
        }
        // Return a Created response with the location of the new role
        return CreatedAtAction(nameof(GetRoleById), new { id = result.Value.RoleId }, result.Value);
    }

    /// <summary>
    /// Updates the details of an existing role identified by the specified ID.
    /// </summary>
    /// <remarks>The caller must have the "roles:write" permission to update a role. The method returns a bad
    /// request if the route ID does not match the command's RoleId, or if the update fails due to invalid data. If the
    /// specified role does not exist, a not found response is returned.</remarks>
    /// <param name="id">The unique identifier of the role to update. Must match the RoleId in the command.</param>
    /// <param name="command">An object containing the updated role information. The RoleId property must correspond to the route parameter.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the operation. Returns <see cref="OkResult"/> with the
    /// updated role on success, <see cref="BadRequestResult"/> if the input is invalid, <see cref="NotFoundResult"/> if
    /// the role does not exist, or <see cref="ForbidResult"/> if the caller lacks permission.</returns>
    [HttpPut("{id}")]
    [RequirePermission("roles:write")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateRoleCommand command)
    {
        // Log the incoming request for debugging purposes
        logger.LogDebug("Updating role: {RoleId}", id);
        // Validate that the route ID matches the command's RoleId
        if (id != command.RoleId)
        {
            // Return a Bad Request response if there is a mismatch
            return BadRequest(new { error = "Role ID in route does not match command" });
        }

        // Send the command to the mediator and await the result
        var result = await mediator.Send(command);
        if (result.IsFailure)
        {
            // Check if the error indicates that the role was not found
            if (result.Error.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { error = result.Error });
            }
            // Return a Bad Request response for other errors
            return BadRequest(new { error = result.Error });
        }
        // Return the successful result
        return Ok(result.Value);
    }

    /// <summary>
    /// Deletes the role identified by the specified unique identifier.
    /// </summary>
    /// <remarks>Requires the 'roles:delete' permission. Returns status code 204 if the deletion is
    /// successful, 404 if the role is not found, 400 for invalid requests, and 403 if the user lacks
    /// permission.</remarks>
    /// <param name="id">The unique identifier of the role to delete.</param>
    /// <returns>A <see cref="NoContentResult"/> if the role was successfully deleted; a <see cref="NotFoundResult"/> if the role
    /// does not exist; or a <see cref="BadRequestResult"/> if the request is invalid.</returns>
    [HttpDelete("{id}")]
    [RequirePermission("roles:delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteRole(Guid id)
    {
        // Log the incoming request for debugging purposes
        logger.LogDebug("Deleting role: {RoleId}", id);
        // Create the command object with the specified role ID
        var command = new DeleteRoleCommand(id);

        // Send the command to the mediator and await the result
        var result = await mediator.Send(command);
        if (result.IsFailure)
        {
            // Check if the error indicates that the role was not found
            if (result.Error.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { error = result.Error });
            }
            // Return a Bad Request response for other errors
            return BadRequest(new { error = result.Error });
        }
        // Return No Content on successful deletion
        return NoContent();
    }

    /// <summary>
    /// Toggles the active status of a role identified by the specified ID.
    /// </summary>
    /// <remarks>Requires the 'roles:manage' permission. This endpoint is accessible via HTTP PATCH at
    /// '{id}/status'.</remarks>
    /// <param name="id">The unique identifier of the role whose status is to be updated.</param>
    /// <param name="request">An object containing the desired active status for the role. Must not be null.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the operation. Returns 200 OK with the updated role
    /// status if successful; 404 Not Found if the role does not exist; or 403 Forbidden if the caller lacks sufficient
    /// permissions.</returns>
    [HttpPatch("{id}/toggle-status")]
    [RequirePermission("roles:manage")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ToggleRoleStatus(Guid id, [FromBody] ToggleRoleStatusRequest request)
    {
        // Log the incoming request for debugging purposes
        logger.LogDebug("Toggling role status: {RoleId}, IsActive: {IsActive}", id, request.IsActive);
        // Create the command object with the specified role ID and desired status
        var command = new ToggleRoleStatusCommand(id, request.IsActive);

        // Send the command to the mediator and await the result
        var result = await mediator.Send(command);
        if (result.IsFailure)
        {
            // Return a Not Found response if the role does not exist
            return NotFound(new { error = result.Error });
        }
        // Return the successful result
        return Ok(result.Value);
    }

    /// <summary>
    /// Grants a set of permissions to a role, replacing all existing permissions.
    /// </summary>
    /// <remarks>Requires the 'roles:write' permission. This endpoint replaces all existing permissions with the
    /// provided set. Returns HTTP 200 OK with the updated role details on success, HTTP 400 Bad Request if any
    /// permission IDs are invalid, HTTP 404 Not Found if the role does not exist, or HTTP 403 Forbidden if the user
    /// lacks permission.</remarks>
    /// <param name="roleId">The unique identifier of the role to grant permissions to.</param>
    /// <param name="request">An object containing the list of permission IDs to grant to the role.</param>
    /// <returns>An <see cref="IActionResult"/> containing the updated role details if successful; otherwise, an error response.</returns>
    [HttpPut("{roleId}/grant-permissions")]
    [RequirePermission("roles:write")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GrantPermissions(Guid roleId, [FromBody] GrantPermissionsRequest request)
    {
        // Log the incoming request for debugging purposes
        logger.LogDebug("Granting permissions to role: {RoleId}, Permission count: {Count}", roleId, request.PermissionIds.Count);
        
        // Create the command object with the role ID and permission IDs
        var command = new GrantPermissionsCommand(roleId, request.PermissionIds);

        // Send the command to the mediator and await the result
        var result = await mediator.Send(command);
        if (result.IsFailure)
        {
            // Check if the error indicates that the role was not found
            if (result.Error.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { error = result.Error });
            }
            // Return a Bad Request response for other errors
            return BadRequest(new { error = result.Error });
        }
        // Return the successful result
        return Ok(result.Value);
    }
}

/// <summary>
/// Represents a request to change the active status of a role.
/// </summary>
/// <param name="IsActive">A value indicating whether the role should be set as active. Specify <see langword="true"/> to activate the role;
/// otherwise, <see langword="false"/> to deactivate it.</param>
public record ToggleRoleStatusRequest(bool IsActive);

/// <summary>
/// Represents a request to grant permissions to a role.
/// </summary>
/// <param name="PermissionIds">The list of permission identifiers to grant to the role.</param>
public record GrantPermissionsRequest(List<Guid> PermissionIds);
