using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VolcanionAuth.API.Filters;
using VolcanionAuth.Application.Features.UserManagement.Commands.AssignRoles;
using VolcanionAuth.Application.Features.UserManagement.Commands.CreateUser;
using VolcanionAuth.Application.Features.UserManagement.Commands.DeleteUser;
using VolcanionAuth.Application.Features.UserManagement.Commands.ToggleUserStatus;
using VolcanionAuth.Application.Features.UserManagement.Commands.UpdateUser;
using VolcanionAuth.Application.Features.UserManagement.Queries.GetAllUsers;
using VolcanionAuth.Application.Features.UserManagement.Queries.GetUserById;

namespace VolcanionAuth.API.Controllers.V1;

/// <summary>
/// Provides API endpoints for managing users in the system.
/// All endpoints require authentication and specific permissions.
/// </summary>
/// <remarks>
/// This controller implements role-based access control (RBAC) to ensure only authorized users
/// can perform user management operations. Required permissions:
/// - users:read: View user information
/// - users:write: Create and update users
/// - users:delete: Delete users
/// - users:manage: Full user management access (includes all user operations)
/// </remarks>
/// <param name="mediator">The mediator used to dispatch commands and queries for user management operations.</param>
/// <param name="logger">The logger used to record diagnostic and operational information for the controller.</param>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class UserManagementController(IMediator mediator, ILogger<UserManagementController> logger) : ControllerBase
{
    /// <summary>
    /// Retrieves a paginated list of users, optionally including inactive users and filtering by a search term.
    /// </summary>
    /// <remarks>Requires the 'users:read' permission. Returns HTTP 200 (OK) with the user list, HTTP 400 (Bad
    /// Request) if the query parameters are invalid, or HTTP 403 (Forbidden) if the caller lacks permission.</remarks>
    /// <param name="page">The page number of results to retrieve. Must be greater than or equal to 1.</param>
    /// <param name="pageSize">The maximum number of users to include in a single page of results. Must be greater than 0.</param>
    /// <param name="includeInactive">Specifies whether to include inactive users in the results. Set to <see langword="true"/> to include inactive
    /// users; otherwise, only active users are returned.</param>
    /// <param name="searchTerm">An optional search term used to filter users by name, email, or other identifying information. If <see
    /// langword="null"/>, no filtering is applied.</param>
    /// <returns>An <see cref="IActionResult"/> containing a paginated list of users if the request is successful; otherwise, a
    /// bad request or forbidden response.</returns>
    [HttpGet]
    [RequirePermission("users:read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool includeInactive = true,
        [FromQuery] string? searchTerm = null)
    {
        // Log the request details
        logger.LogDebug("Getting all users - Page: {Page}, PageSize: {PageSize}, IncludeInactive: {IncludeInactive}, SearchTerm: {SearchTerm}", page, pageSize, includeInactive, searchTerm);
        // Prepare the query for retrieving users
        var query = new GetAllUsersQuery(page, pageSize, includeInactive, searchTerm);

        // Send the query via the mediator
        var result = await mediator.Send(query);
        if (result.IsFailure)
        {
            // Return a bad request response if the query fails
            return BadRequest(new { error = result.Error });
        }
        // Return the list of users
        return Ok(result.Value);
    }

    /// <summary>
    /// Retrieves the details of a user specified by their unique identifier.
    /// </summary>
    /// <remarks>Returns a 200 OK response with the user details if the user exists and the caller has
    /// permission. Returns a 404 Not Found response if the user does not exist, or a 403 Forbidden response if the
    /// caller lacks the required permission.</remarks>
    /// <param name="id">The unique identifier of the user to retrieve.</param>
    /// <returns>An <see cref="IActionResult"/> containing the user details if found; otherwise, a response indicating that the
    /// user was not found or access is forbidden.</returns>
    [HttpGet("{id}")]
    [RequirePermission("users:read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        // Log the request
        logger.LogDebug("Getting user by ID: {UserId}", id);
        // Prepare the query to get the user by ID
        var query = new GetUserByIdQuery(id);

        // Send the query via the mediator
        var result = await mediator.Send(query);
        if (result.IsFailure)
        {
            // Return a not found response if the user does not exist
            return NotFound(new { error = result.Error });
        }
        // Return the user details
        return Ok(result.Value);
    }

    /// <summary>
    /// Creates a new user account using the specified registration details.
    /// </summary>
    /// <remarks>This action requires the 'users:write' permission. If a user with the specified email already
    /// exists, a 409 Conflict response is returned. Validation errors result in a 400 Bad Request response.</remarks>
    /// <param name="command">The command containing the user's registration information, such as email and password. Cannot be null.</param>
    /// <returns>A result indicating the outcome of the operation. Returns a 201 Created response with the new user's details if
    /// successful; otherwise, returns a 400 Bad Request, 403 Forbidden, or 409 Conflict response depending on the error
    /// condition.</returns>
    [HttpPost]
    [RequirePermission("users:write")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command)
    {
        // Log the request details
        logger.LogDebug("Creating user with email: {Email}", command.Email);

        // Send the command via the mediator
        var result = await mediator.Send(command);
        if (result.IsFailure)
        {
            // Handle specific error cases
            if (result.Error.Contains("email already exists", StringComparison.OrdinalIgnoreCase))
            {
                return Conflict(new { error = result.Error });
            }
            // Return a bad request response for other errors
            return BadRequest(new { error = result.Error });
        }
        // Return a created response with the new user's details
        return CreatedAtAction(nameof(GetUserById), new { id = result.Value.UserId }, result.Value);
    }

    /// <summary>
    /// Updates the details of an existing user with the specified identifier.
    /// </summary>
    /// <remarks>Requires the "users:write" permission. Returns HTTP 200 (OK) on success, 400 (Bad Request)
    /// for validation errors or mismatched IDs, 404 (Not Found) if the user does not exist, and 403 (Forbidden) if the
    /// caller is not authorized.</remarks>
    /// <param name="id">The unique identifier of the user to update. Must match the user ID in the command.</param>
    /// <param name="command">An object containing the updated user information. The user ID in the command must match the route parameter.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the update operation. Returns <see
    /// cref="OkObjectResult"/> with the updated user on success, <see cref="BadRequestObjectResult"/> if the request is
    /// invalid, <see cref="NotFoundObjectResult"/> if the user does not exist, or <see cref="ForbidResult"/> if the
    /// caller lacks permission.</returns>
    [HttpPut("{id}")]
    [RequirePermission("users:write")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserCommand command)
    {
        // Log the request details
        logger.LogDebug("Updating user: {UserId}", id);
        // Validate that the route ID matches the command's user ID
        if (id != command.UserId)
        {
            return BadRequest(new { error = "User ID in route does not match command" });
        }

        // Send the command via the mediator
        var result = await mediator.Send(command);
        if (result.IsFailure)
        {
            // Handle specific error cases
            if (result.Error.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { error = result.Error });
            }
            // Return a bad request response for other errors
            return BadRequest(new { error = result.Error });
        }
        // Return the updated user details
        return Ok(result.Value);
    }

    /// <summary>
    /// Deletes the user with the specified unique identifier.
    /// </summary>
    /// <remarks>Requires the "users:delete" permission. Returns HTTP 204 if successful, 404 if the user is
    /// not found, or 403 if access is forbidden.</remarks>
    /// <param name="id">The unique identifier of the user to delete.</param>
    /// <returns>A <see cref="NoContentResult"/> if the user was successfully deleted; a <see cref="NotFoundObjectResult"/> if
    /// the user does not exist; or a <see cref="ForbidResult"/> if the caller lacks sufficient permissions.</returns>
    [HttpDelete("{id}")]
    [RequirePermission("users:delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        // Log the request details
        logger.LogDebug("Deleting user: {UserId}", id);
        // Prepare the command to delete the user
        var command = new DeleteUserCommand(id);

        // Send the command via the mediator
        var result = await mediator.Send(command);
        if (result.IsFailure)
        {
            // Return a not found response if the user does not exist
            return NotFound(new { error = result.Error });
        }
        // Return a no content response on successful deletion
        return NoContent();
    }

    /// <summary>
    /// Toggles the active status of the specified user account.
    /// </summary>
    /// <remarks>Requires the 'users:manage' permission. This endpoint is typically used by administrators to
    /// enable or disable user accounts.</remarks>
    /// <param name="id">The unique identifier of the user whose status is to be toggled.</param>
    /// <param name="request">An object containing the desired active status for the user. Must not be null.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the operation. Returns 200 OK with the updated user
    /// status if successful; 404 Not Found if the user does not exist; or 403 Forbidden if the caller lacks sufficient
    /// permissions.</returns>
    [HttpPatch("{id}/toggle-status")]
    [RequirePermission("users:manage")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ToggleUserStatus(Guid id, [FromBody] ToggleUserStatusRequest request)
    {
        // Log the request details
        logger.LogDebug("Toggling user status: {UserId}, IsActive: {IsActive}", id, request.IsActive);
        // Prepare the command to toggle the user's status
        var command = new ToggleUserStatusCommand(id, request.IsActive);

        // Send the command via the mediator
        var result = await mediator.Send(command);
        if (result.IsFailure)
        {
            // Return a not found response if the user does not exist
            return NotFound(new { error = result.Error });
        }
        // Return the updated user status
        return Ok(result.Value);
    }

    /// <summary>
    /// Assigns a set of roles to a user, replacing all existing role assignments.
    /// </summary>
    /// <remarks>Requires the 'users:write' permission. This endpoint replaces all existing roles with the
    /// provided set. Only active roles can be assigned. Returns HTTP 200 OK with the updated user details on success,
    /// HTTP 400 Bad Request if any role IDs are invalid or roles are inactive, HTTP 404 Not Found if the user does not
    /// exist, or HTTP 403 Forbidden if the caller lacks permission.</remarks>
    /// <param name="userId">The unique identifier of the user to assign roles to.</param>
    /// <param name="request">An object containing the list of role IDs to assign to the user.</param>
    /// <returns>An <see cref="IActionResult"/> containing the updated user details if successful; otherwise, an error response.</returns>
    [HttpPut("{userId}/assign-roles")]
    [RequirePermission("users:write")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AssignRoles(Guid userId, [FromBody] AssignRolesRequest request)
    {
        // Log the request details
        logger.LogDebug("Assigning roles to user: {UserId}, Role count: {Count}", userId, request.RoleIds.Count);
        
        // Create the command object with the user ID and role IDs
        var command = new AssignRolesCommand(userId, request.RoleIds);

        // Send the command via the mediator
        var result = await mediator.Send(command);
        if (result.IsFailure)
        {
            // Check if the error indicates that the user was not found
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
/// Represents a request to change a user's active status.
/// </summary>
/// <param name="IsActive">A value indicating whether the user should be set as active. Specify <see langword="true"/> to activate the user;
/// otherwise, <see langword="false"/> to deactivate.</param>
public record ToggleUserStatusRequest(bool IsActive);

/// <summary>
/// Represents a request to assign roles to a user.
/// </summary>
/// <param name="RoleIds">The list of role identifiers to assign to the user.</param>
public record AssignRolesRequest(List<Guid> RoleIds);
