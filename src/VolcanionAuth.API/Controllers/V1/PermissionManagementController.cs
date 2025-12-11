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
/// Provides API endpoints for managing permissions, including retrieving, creating, and deleting permission records.
/// Requires authentication and appropriate permissions for access.
/// </summary>
/// <remarks>All endpoints in this controller require the caller to be authenticated and to possess the necessary
/// permission scopes. Responses include standard HTTP status codes to indicate success or failure, such as 200 OK, 201
/// Created, 204 No Content, 400 Bad Request, 403 Forbidden, 404 Not Found, and 409 Conflict. This controller is
/// versioned as part of the API and is intended for use in administrative scenarios where permission management is
/// required.</remarks>
/// <param name="mediator">The mediator used to send commands and queries for permission management operations.</param>
/// <param name="logger">The logger used to record diagnostic and operational information for permission management actions.</param>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class PermissionManagementController(IMediator mediator, ILogger<PermissionManagementController> logger) : ControllerBase
{
    /// <summary>
    /// Retrieves a paginated list of permissions, optionally filtered by resource and search term.
    /// </summary>
    /// <remarks>Requires the 'permissions:read' permission. This endpoint supports pagination and filtering
    /// to efficiently retrieve large sets of permissions.</remarks>
    /// <param name="page">The page number of results to retrieve. Must be greater than zero. Defaults to 1.</param>
    /// <param name="pageSize">The number of permissions to include per page. Must be greater than zero. Defaults to 10.</param>
    /// <param name="resource">An optional resource name to filter permissions by. If null, permissions for all resources are included.</param>
    /// <param name="searchTerm">An optional search term to filter permissions by name or description. If null, no search filtering is applied.</param>
    /// <returns>An IActionResult containing a list of permissions in the specified page. Returns 200 OK with the results, 400
    /// Bad Request if the parameters are invalid, or 403 Forbidden if the caller lacks the required permission.</returns>
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
        // Log the request details
        logger.LogDebug("Getting all permissions - Page: {Page}, PageSize: {PageSize}", page, pageSize);
        // Create the query object with the provided parameters
        var query = new GetAllPermissionsQuery(page, pageSize, resource, searchTerm);

        // Send the query to the mediator for processing
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
    /// Retrieves all permissions grouped by their associated resource.
    /// </summary>
    /// <remarks>Returns a 200 OK response with the grouped permissions on success. Returns a 400 Bad Request
    /// if the request is invalid, or a 403 Forbidden if the caller does not have sufficient permissions. This endpoint
    /// requires the "permissions:read" permission.</remarks>
    /// <returns>An <see cref="IActionResult"/> containing the grouped permissions if the request is successful; otherwise, a
    /// response indicating the error condition.</returns>
    [HttpGet("grouped-by-resource")]
    [RequirePermission("permissions:read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPermissionsGroupedByResource()
    {
        // Log the request
        logger.LogDebug("Getting all permissions grouped by resource");
        // Create the query object
        var query = new GetPermissionsGroupedByResourceQuery();

        // Send the query to the mediator
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
    /// Retrieves the details of a permission identified by the specified unique identifier.
    /// </summary>
    /// <remarks>Requires the 'permissions:read' permission. Returns status code 200 if the permission is
    /// found, 404 if not found, and 403 if the caller lacks sufficient permissions.</remarks>
    /// <param name="id">The unique identifier of the permission to retrieve.</param>
    /// <returns>An <see cref="IActionResult"/> containing the permission details if found; otherwise, a response with status
    /// code 404 if the permission does not exist, or 403 if access is denied.</returns>
    [HttpGet("{id}")]
    [RequirePermission("permissions:read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPermissionById(Guid id)
    {
        // Log the request
        logger.LogDebug("Getting permission by ID: {PermissionId}", id);
        // Create the query object
        var query = new GetPermissionByIdQuery(id);

        // Send the query to the mediator
        var result = await mediator.Send(query);
        if (result.IsFailure)
        {
            // Return a Not Found response if the permission does not exist
            return NotFound(new { error = result.Error });
        }
        // Return the successful result
        return Ok(result.Value);
    }

    /// <summary>
    /// Creates a new permission based on the specified command.
    /// </summary>
    /// <remarks>Requires the 'permissions:write' permission. The created permission can be retrieved using
    /// its identifier from the response.</remarks>
    /// <param name="command">The command containing the details of the permission to create. Must not be null.</param>
    /// <returns>A result that indicates the outcome of the operation. Returns 201 Created with the created permission if
    /// successful; 400 Bad Request if the command is invalid; 403 Forbidden if the caller lacks required permissions;
    /// or 409 Conflict if a permission with the same resource and action already exists.</returns>
    [HttpPost]
    [RequirePermission("permissions:write")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreatePermission([FromBody] CreatePermissionCommand command)
    {
        // Log the request details
        logger.LogDebug("Creating permission: {Resource}:{Action}", command.Resource, command.Action);

        // Send the command to the mediator for processing
        var result = await mediator.Send(command);
        if (result.IsFailure)
        {
            // Handle conflict if the permission already exists
            if (result.Error.Contains("already exists", StringComparison.OrdinalIgnoreCase))
            {
                // Return a Conflict response if the permission already exists
                return Conflict(new { error = result.Error });
            }
            // Return a Bad Request response for other errors
            return BadRequest(new { error = result.Error });
        }
        // Return the created permission with a 201 Created response
        return CreatedAtAction(nameof(GetPermissionById), new { id = result.Value.PermissionId }, result.Value);
    }

    /// <summary>
    /// Deletes the permission identified by the specified ID.
    /// </summary>
    /// <remarks>Requires the 'permissions:delete' permission. The caller must be authorized to perform this
    /// operation.</remarks>
    /// <param name="id">The unique identifier of the permission to delete.</param>
    /// <returns>A 204 No Content response if the permission was successfully deleted; a 404 Not Found response if the permission
    /// does not exist; a 400 Bad Request response if the request is invalid; or a 403 Forbidden response if the caller
    /// does not have sufficient permissions.</returns>
    [HttpDelete("{id}")]
    [RequirePermission("permissions:delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeletePermission(Guid id)
    {
        // Log the request
        logger.LogDebug("Deleting permission: {PermissionId}", id);
        // Create the command object
        var command = new DeletePermissionCommand(id);

        // Send the command to the mediator
        var result = await mediator.Send(command);
        if (result.IsFailure)
        {
            // Handle not found error
            if (result.Error.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                // Return a Not Found response if the permission does not exist
                return NotFound(new { error = result.Error });
            }
            // Return a Bad Request response for other errors
            return BadRequest(new { error = result.Error });
        }
        // Return No Content on successful deletion
        return NoContent();
    }
}
