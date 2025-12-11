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
/// Provides API endpoints for managing access control policies, including operations to retrieve, create, update,
/// delete, and toggle the status of policies.
/// </summary>
/// <remarks>All endpoints require appropriate permissions and authorization. The controller supports versioned
/// API routes and returns standard HTTP status codes for success and error conditions. Pagination, filtering, and
/// search are available for policy retrieval operations.</remarks>
/// <param name="mediator">The mediator used to dispatch commands and queries related to policy management.</param>
/// <param name="logger">The logger used to record diagnostic and operational information for the controller.</param>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class PolicyManagementController(IMediator mediator, ILogger<PolicyManagementController> logger) : ControllerBase
{
    /// <summary>
    /// Retrieves a paginated list of policies, optionally including inactive policies and filtered by resource or
    /// search term.
    /// </summary>
    /// <remarks>Requires the 'policies:read' permission. Returns HTTP 200 (OK) with the list of policies,
    /// HTTP 400 (Bad Request) if the request parameters are invalid, or HTTP 403 (Forbidden) if the user does not have
    /// sufficient permissions.</remarks>
    /// <param name="page">The page number of results to retrieve. Must be greater than or equal to 1.</param>
    /// <param name="pageSize">The maximum number of policies to include in a single page of results. Must be greater than 0.</param>
    /// <param name="includeInactive">Specifies whether to include inactive policies in the results. Set to <see langword="true"/> to include inactive
    /// policies; otherwise, only active policies are returned.</param>
    /// <param name="resource">An optional resource identifier to filter policies by their associated resource. If <see langword="null"/>, no
    /// resource filtering is applied.</param>
    /// <param name="searchTerm">An optional search term to filter policies by name or description. If <see langword="null"/>, no search
    /// filtering is applied.</param>
    /// <returns>An <see cref="IActionResult"/> containing a paginated list of policies if successful; otherwise, a Bad Request
    /// or Forbidden response if the request is invalid or the user lacks permission.</returns>
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
        // Log the request details
        logger.LogDebug("Getting all policies - Page: {Page}, PageSize: {PageSize}", page, pageSize);
        // Create and send the query to retrieve policies
        var query = new GetAllPoliciesQuery(page, pageSize, includeInactive, resource, searchTerm);

        // Send the query via the mediator
        var result = await mediator.Send(query);
        if (result.IsFailure)
        {
            // Return a Bad Request response if the query fails
            return BadRequest(new { error = result.Error });
        }
        // Return the list of policies in an OK response
        return Ok(result.Value);
    }

    /// <summary>
    /// Retrieves a policy by its unique identifier.
    /// </summary>
    /// <remarks>Requires the "policies:read" permission. Returns status code 200 if the policy is found, 404
    /// if not found, and 403 if access is forbidden.</remarks>
    /// <param name="id">The unique identifier of the policy to retrieve.</param>
    /// <returns>An <see cref="IActionResult"/> containing the policy data if found; otherwise, a response with status code 404
    /// if the policy does not exist, or 403 if the caller lacks permission.</returns>
    [HttpGet("{id}")]
    [RequirePermission("policies:read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPolicyById(Guid id)
    {
        // Log the request
        logger.LogDebug("Getting policy by ID: {PolicyId}", id);
        // Create and send the query to retrieve the policy by ID
        var query = new GetPolicyByIdQuery(id);

        // Send the query via the mediator
        var result = await mediator.Send(query);
        if (result.IsFailure)
        {
            // Return a Not Found response if the policy does not exist
            return NotFound(new { error = result.Error });
        }
        // Return the policy data in an OK response
        return Ok(result.Value);
    }

    /// <summary>
    /// Creates a new policy using the specified command data.
    /// </summary>
    /// <remarks>Requires the 'policies:write' permission. Returns a 409 Conflict response if a policy with
    /// the same name already exists.</remarks>
    /// <param name="command">The command containing the details of the policy to create. Must not be null and must include all required
    /// policy information.</param>
    /// <returns>A result that indicates the outcome of the operation. Returns a 201 Created response with the created policy if
    /// successful; otherwise, returns a 400 Bad Request, 403 Forbidden, or 409 Conflict response depending on the error
    /// condition.</returns>
    [HttpPost]
    [RequirePermission("policies:write")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreatePolicy([FromBody] CreatePolicyCommand command)
    {
        // Log the request
        logger.LogDebug("Creating policy: {Name}", command.Name);

        // Send the create policy command via the mediator
        var result = await mediator.Send(command);
        if (result.IsFailure)
        {
            // Handle specific error conditions
            if (result.Error.Contains("already exists", StringComparison.OrdinalIgnoreCase))
            {
                return Conflict(new { error = result.Error });
            }
            // Return a Bad Request response for other errors
            return BadRequest(new { error = result.Error });
        }
        // Return a Created response with the location of the new policy
        return CreatedAtAction(nameof(GetPolicyById), new { id = result.Value.PolicyId }, result.Value);
    }

    /// <summary>
    /// Updates an existing policy with the specified changes.
    /// </summary>
    /// <remarks>Requires the 'policies:write' permission. Returns HTTP 200 if the update succeeds, 400 for
    /// invalid input, 404 if the policy is not found, and 403 if access is forbidden.</remarks>
    /// <param name="id">The unique identifier of the policy to update. Must match the PolicyId in <paramref name="command"/>.</param>
    /// <param name="command">An object containing the updated policy data. The PolicyId property must match the <paramref name="id"/>
    /// parameter.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the operation. Returns <see cref="OkResult"/> with the
    /// updated policy if successful; <see cref="BadRequestResult"/> if the input is invalid; <see
    /// cref="NotFoundResult"/> if the policy does not exist; or <see cref="ForbidResult"/> if the caller lacks
    /// permission.</returns>
    [HttpPut("{id}")]
    [RequirePermission("policies:write")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdatePolicy(Guid id, [FromBody] UpdatePolicyCommand command)
    {
        // Log the request
        logger.LogDebug("Updating policy: {PolicyId}", id);
        // Validate that the route ID matches the command's PolicyId
        if (id != command.PolicyId)
        {
            return BadRequest(new { error = "Policy ID in route does not match command" });
        }

        // Send the update policy command via the mediator
        var result = await mediator.Send(command);
        if (result.IsFailure)
        {
            // Handle specific error conditions
            if (result.Error.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { error = result.Error });
            }
            // Return a Bad Request response for other errors
            return BadRequest(new { error = result.Error });
        }
        // Return the updated policy in an OK response
        return Ok(result.Value);
    }

    /// <summary>
    /// Deletes the policy identified by the specified ID.
    /// </summary>
    /// <remarks>Requires the caller to have the "policies:delete" permission. Returns a 403 Forbidden
    /// response if the caller lacks sufficient permissions.</remarks>
    /// <param name="id">The unique identifier of the policy to delete.</param>
    /// <returns>A <see cref="NoContentResult"/> if the policy was successfully deleted; a <see cref="NotFoundResult"/> if the
    /// policy does not exist; or a <see cref="BadRequestResult"/> if the request is invalid.</returns>
    [HttpDelete("{id}")]
    [RequirePermission("policies:delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeletePolicy(Guid id)
    {
        // Log the request
        logger.LogDebug("Deleting policy: {PolicyId}", id);
        // Create and send the delete policy command
        var command = new DeletePolicyCommand(id);

        // Send the command via the mediator
        var result = await mediator.Send(command);
        if (result.IsFailure)
        {
            // Handle specific error conditions
            if (result.Error.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { error = result.Error });
            }
            // Return a Bad Request response for other errors
            return BadRequest(new { error = result.Error });
        }
        // Return No Content response on successful deletion
        return NoContent();
    }

    /// <summary>
    /// Toggles the active status of the specified policy.
    /// </summary>
    /// <remarks>Requires the 'policies:manage' permission. Only users with appropriate authorization can
    /// toggle the status of a policy.</remarks>
    /// <param name="id">The unique identifier of the policy to update.</param>
    /// <param name="request">An object containing the desired active status for the policy.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the operation. Returns 200 OK with the updated policy
    /// status if successful; 404 Not Found if the policy does not exist; or 403 Forbidden if the user lacks sufficient
    /// permissions.</returns>
    [HttpPatch("{id}/toggle-status")]
    [RequirePermission("policies:manage")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> TogglePolicyStatus(Guid id, [FromBody] TogglePolicyStatusRequest request)
    {
        // Log the request
        logger.LogDebug("Toggling policy status: {PolicyId}, IsActive: {IsActive}", id, request.IsActive);
        // Create and send the toggle policy status command
        var command = new TogglePolicyStatusCommand(id, request.IsActive);

        // Send the command via the mediator
        var result = await mediator.Send(command);
        if (result.IsFailure)
        {
            // Return a Not Found response if the policy does not exist
            return NotFound(new { error = result.Error });
        }
        // Return the updated policy in an OK response
        return Ok(result.Value);
    }
}

/// <summary>
/// Represents a request to change the active status of a policy.
/// </summary>
/// <param name="IsActive">A value indicating whether the policy should be set as active. Specify <see langword="true"/> to activate the
/// policy; otherwise, <see langword="false"/> to deactivate it.</param>
public record TogglePolicyStatusRequest(bool IsActive);
