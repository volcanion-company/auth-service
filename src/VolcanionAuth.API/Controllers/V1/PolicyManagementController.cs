using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VolcanionAuth.API.Filters;
using VolcanionAuth.Application.Features.PolicyManagement.Commands.CreatePolicy;
using VolcanionAuth.Application.Features.PolicyManagement.Commands.DeletePolicy;
using VolcanionAuth.Application.Features.PolicyManagement.Commands.TogglePolicyStatus;
using VolcanionAuth.Application.Features.PolicyManagement.Commands.UpdatePolicy;
using VolcanionAuth.Application.Features.PolicyManagement.Queries.GetAllPolicies;
using VolcanionAuth.Application.Features.PolicyManagement.Queries.GetPolicyById;

namespace VolcanionAuth.API.Controllers.V1;

/// <summary>
/// Provides API endpoints for managing policies in the system.
/// All endpoints require authentication and specific permissions.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class PolicyManagementController(IMediator mediator, ILogger<PolicyManagementController> logger) : ControllerBase
{
    /// <summary>
    /// Retrieves a paginated list of all policies in the system.
    /// </summary>
    [HttpGet]
    [RequirePermission("policies:read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllPolicies(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool includeInactive = false,
        [FromQuery] string? resource = null,
        [FromQuery] string? searchTerm = null)
    {
        logger.LogDebug("Getting all policies - Page: {Page}, PageSize: {PageSize}", page, pageSize);

        var query = new GetAllPoliciesQuery(page, pageSize, includeInactive, resource, searchTerm);
        var result = await mediator.Send(query);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Retrieves detailed information about a specific policy by ID.
    /// </summary>
    [HttpGet("{id}")]
    [RequirePermission("policies:read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPolicyById(Guid id)
    {
        logger.LogDebug("Getting policy by ID: {PolicyId}", id);

        var query = new GetPolicyByIdQuery(id);
        var result = await mediator.Send(query);

        if (result.IsFailure)
        {
            return NotFound(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Creates a new policy in the system.
    /// </summary>
    [HttpPost]
    [RequirePermission("policies:write")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreatePolicy([FromBody] CreatePolicyCommand command)
    {
        logger.LogDebug("Creating policy: {Name}", command.Name);

        var result = await mediator.Send(command);

        if (result.IsFailure)
        {
            if (result.Error.Contains("already exists", StringComparison.OrdinalIgnoreCase))
            {
                return Conflict(new { error = result.Error });
            }
            return BadRequest(new { error = result.Error });
        }

        return CreatedAtAction(nameof(GetPolicyById), new { id = result.Value.PolicyId }, result.Value);
    }

    /// <summary>
    /// Updates an existing policy's information.
    /// </summary>
    [HttpPut("{id}")]
    [RequirePermission("policies:write")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdatePolicy(Guid id, [FromBody] UpdatePolicyCommand command)
    {
        logger.LogDebug("Updating policy: {PolicyId}", id);

        if (id != command.PolicyId)
        {
            return BadRequest(new { error = "Policy ID in route does not match command" });
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
    /// Permanently deletes a policy from the system.
    /// </summary>
    [HttpDelete("{id}")]
    [RequirePermission("policies:delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeletePolicy(Guid id)
    {
        logger.LogDebug("Deleting policy: {PolicyId}", id);

        var command = new DeletePolicyCommand(id);
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
    /// Activates or deactivates a policy.
    /// </summary>
    [HttpPatch("{id}/status")]
    [RequirePermission("policies:manage")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> TogglePolicyStatus(Guid id, [FromBody] TogglePolicyStatusRequest request)
    {
        logger.LogDebug("Toggling policy status: {PolicyId}, IsActive: {IsActive}", id, request.IsActive);

        var command = new TogglePolicyStatusCommand(id, request.IsActive);
        var result = await mediator.Send(command);

        if (result.IsFailure)
        {
            return NotFound(new { error = result.Error });
        }

        return Ok(result.Value);
    }
}

public record TogglePolicyStatusRequest(bool IsActive);
