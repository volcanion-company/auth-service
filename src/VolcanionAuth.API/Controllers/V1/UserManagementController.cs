using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VolcanionAuth.API.Filters;
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
    /// Retrieves a paginated list of all users in the system.
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 10, max: 100)</param>
    /// <param name="includeInactive">Whether to include inactive users (default: false)</param>
    /// <param name="searchTerm">Optional search term to filter users by name or email</param>
    /// <returns>A paginated list of users with their roles</returns>
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
        logger.LogDebug("Getting all users - Page: {Page}, PageSize: {PageSize}, IncludeInactive: {IncludeInactive}, SearchTerm: {SearchTerm}",
            page, pageSize, includeInactive, searchTerm);

        var query = new GetAllUsersQuery(page, pageSize, includeInactive, searchTerm);
        var result = await mediator.Send(query);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Retrieves detailed information about a specific user by their ID.
    /// </summary>
    /// <param name="id">The unique identifier of the user</param>
    /// <returns>Detailed user information including roles</returns>
    [HttpGet("{id}")]
    [RequirePermission("users:read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        logger.LogDebug("Getting user by ID: {UserId}", id);

        var query = new GetUserByIdQuery(id);
        var result = await mediator.Send(query);

        if (result.IsFailure)
        {
            return NotFound(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Creates a new user in the system.
    /// </summary>
    /// <param name="command">The command containing the user details to create</param>
    /// <returns>The created user's information</returns>
    [HttpPost]
    [RequirePermission("users:write")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command)
    {
        logger.LogDebug("Creating user with email: {Email}", command.Email);

        var result = await mediator.Send(command);

        if (result.IsFailure)
        {
            if (result.Error.Contains("email already exists", StringComparison.OrdinalIgnoreCase))
            {
                return Conflict(new { error = result.Error });
            }
            return BadRequest(new { error = result.Error });
        }

        return CreatedAtAction(nameof(GetUserById), new { id = result.Value.UserId }, result.Value);
    }

    /// <summary>
    /// Updates an existing user's information.
    /// </summary>
    /// <param name="id">The unique identifier of the user to update</param>
    /// <param name="command">The command containing the updated user information</param>
    /// <returns>The updated user's information</returns>
    [HttpPut("{id}")]
    [RequirePermission("users:write")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserCommand command)
    {
        logger.LogDebug("Updating user: {UserId}", id);

        // Ensure the ID in the route matches the ID in the command
        if (id != command.UserId)
        {
            return BadRequest(new { error = "User ID in route does not match command" });
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
    /// Permanently deletes a user from the system.
    /// </summary>
    /// <param name="id">The unique identifier of the user to delete</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id}")]
    [RequirePermission("users:delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        logger.LogDebug("Deleting user: {UserId}", id);

        var command = new DeleteUserCommand(id);
        var result = await mediator.Send(command);

        if (result.IsFailure)
        {
            return NotFound(new { error = result.Error });
        }

        return NoContent();
    }

    /// <summary>
    /// Activates or deactivates a user account.
    /// </summary>
    /// <param name="id">The unique identifier of the user</param>
    /// <param name="request">Request containing the desired active status</param>
    /// <returns>The updated user status</returns>
    [HttpPatch("{id}/toggle-status")]
    [RequirePermission("users:manage")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ToggleUserStatus(Guid id, [FromBody] ToggleUserStatusRequest request)
    {
        logger.LogDebug("Toggling user status: {UserId}, IsActive: {IsActive}", id, request.IsActive);

        var command = new ToggleUserStatusCommand(id, request.IsActive);
        var result = await mediator.Send(command);

        if (result.IsFailure)
        {
            return NotFound(new { error = result.Error });
        }

        return Ok(result.Value);
    }
}

/// <summary>
/// Request model for toggling user status.
/// </summary>
/// <param name="IsActive">True to activate, false to deactivate</param>
public record ToggleUserStatusRequest(bool IsActive);
