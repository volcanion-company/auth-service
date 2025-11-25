using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VolcanionAuth.Application.Features.Authorization.Commands.AssignRole;
using VolcanionAuth.Application.Features.Authorization.Commands.CreatePolicy;
using VolcanionAuth.Application.Features.Authorization.Commands.CreateRole;
using VolcanionAuth.Application.Features.Authorization.Queries.EvaluatePolicy;
using VolcanionAuth.Application.Features.Authorization.Queries.GetUserPermissions;

namespace VolcanionAuth.API.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class AuthorizationController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthorizationController> _logger;

    public AuthorizationController(IMediator mediator, ILogger<AuthorizationController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Create a new role
    /// </summary>
    [HttpPost("roles")]
    [ProducesResponseType(typeof(CreateRoleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleCommand command)
    {
        var result = await _mediator.Send(command);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Assign role to user
    /// </summary>
    [HttpPost("users/{userId}/roles/{roleId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AssignRole(Guid userId, Guid roleId)
    {
        var command = new AssignRoleCommand(userId, roleId);
        var result = await _mediator.Send(command);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(new { message = "Role assigned successfully" });
    }

    /// <summary>
    /// Create a new policy
    /// </summary>
    [HttpPost("policies")]
    [ProducesResponseType(typeof(CreatePolicyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePolicy([FromBody] CreatePolicyCommand command)
    {
        var result = await _mediator.Send(command);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Get user permissions
    /// </summary>
    [HttpGet("users/{userId}/permissions")]
    [ProducesResponseType(typeof(GetUserPermissionsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserPermissions(Guid userId)
    {
        var query = new GetUserPermissionsQuery(userId);
        var result = await _mediator.Send(query);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Evaluate policy for authorization decision
    /// </summary>
    [HttpPost("evaluate")]
    [ProducesResponseType(typeof(EvaluatePolicyResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> EvaluatePolicy([FromBody] EvaluatePolicyQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result.Value);
    }
}
