using System.Net.Mime;
using Emp.ApiGateway.Application.Features.Milestones.Commands;
using Emp.ApiGateway.Application.Features.Milestones.Queries;
using EnterpriseMediator.Contracts.DTOs.Projects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Emp.ApiGateway.Web.Controllers;

/// <summary>
/// Manages project milestones. Includes authenticated endpoints for listing
/// and public endpoints for client milestone approval via secure links.
/// </summary>
[ApiController]
[Produces(MediaTypeNames.Application.Json)]
public class MilestonesController : ControllerBase
{
    private readonly ISender _mediator;
    private readonly ILogger<MilestonesController> _logger;

    public MilestonesController(ISender mediator, ILogger<MilestonesController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves all milestones for a project.
    /// </summary>
    /// <response code="200">Milestones retrieved.</response>
    [HttpGet("api/v1/projects/{projectId:guid}/milestones")]
    [Authorize]
    [ProducesResponseType(typeof(IReadOnlyList<MilestoneDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<MilestoneDto>>> GetProjectMilestones(
        [FromRoute] Guid projectId,
        CancellationToken ct)
    {
        _logger.LogDebug("Retrieving milestones for project: {ProjectId}", projectId);
        var result = await _mediator.Send(new GetProjectMilestonesQuery(projectId), ct);
        return Ok(result);
    }

    /// <summary>
    /// Public endpoint for client milestone approval via secure link.
    /// No [Authorize] — uses token-based verification at the downstream service.
    /// </summary>
    /// <response code="200">Milestone approved.</response>
    /// <response code="404">Milestone not found.</response>
    [HttpPut("api/v1/public/milestones/{milestoneId:guid}/approve")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(MilestoneDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MilestoneDto>> ApproveMilestone(
        [FromRoute] Guid milestoneId,
        CancellationToken ct)
    {
        _logger.LogInformation("Public milestone approval for: {MilestoneId}", milestoneId);
        var result = await _mediator.Send(new ApproveMilestoneCommand(milestoneId), ct);
        return Ok(result);
    }

    /// <summary>
    /// Public endpoint for client milestone rejection via secure link.
    /// No [Authorize] — uses token-based verification at the downstream service.
    /// </summary>
    /// <response code="200">Milestone rejected.</response>
    /// <response code="404">Milestone not found.</response>
    [HttpPut("api/v1/public/milestones/{milestoneId:guid}/reject")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(MilestoneDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MilestoneDto>> RejectMilestone(
        [FromRoute] Guid milestoneId,
        [FromBody] RejectMilestoneRequest request,
        CancellationToken ct)
    {
        _logger.LogInformation("Public milestone rejection for: {MilestoneId}", milestoneId);
        var result = await _mediator.Send(new RejectMilestoneCommand(milestoneId, request.Reason), ct);
        return Ok(result);
    }
}

public record RejectMilestoneRequest(string Reason);
