using System.Net.Mime;
using Emp.ApiGateway.Application.Features.Briefs.Commands;
using Emp.ApiGateway.Application.Features.Briefs.Queries;
using Emp.ApiGateway.Application.Features.Vendors.Queries;
using EnterpriseMediator.Contracts.DTOs.Projects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Emp.ApiGateway.Web.Controllers;

/// <summary>
/// Manages SOW brief review, approval, and vendor matching workflows.
/// </summary>
[ApiController]
[Route("api/v1/projects/{projectId:guid}/briefs")]
[Authorize]
[Produces(MediaTypeNames.Application.Json)]
public class BriefsController : ControllerBase
{
    private readonly ISender _mediator;
    private readonly ILogger<BriefsController> _logger;

    public BriefsController(ISender mediator, ILogger<BriefsController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves the AI-extracted project brief for review.
    /// </summary>
    /// <response code="200">Brief retrieved successfully.</response>
    /// <response code="404">Brief not found for this project.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ProjectBriefDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectBriefDto>> GetProjectBrief(
        [FromRoute] Guid projectId,
        CancellationToken ct)
    {
        _logger.LogDebug("Retrieving brief for project: {ProjectId}", projectId);

        var result = await _mediator.Send(new GetProjectBriefQuery(projectId), ct);

        if (result == null)
        {
            return NotFound($"Brief not found for project {projectId}.");
        }

        return Ok(result);
    }

    /// <summary>
    /// Approves the AI-extracted brief, optionally with admin edits.
    /// Triggers the vendor matching workflow.
    /// </summary>
    /// <response code="200">Brief approved and vendor matching triggered.</response>
    /// <response code="404">Brief not found for this project.</response>
    [HttpPut("approve")]
    [ProducesResponseType(typeof(ProjectBriefDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectBriefDto>> ApproveBrief(
        [FromRoute] Guid projectId,
        [FromBody] UpdateProjectBriefRequest? edits,
        CancellationToken ct)
    {
        _logger.LogInformation("Approving brief for project: {ProjectId}", projectId);

        var result = await _mediator.Send(new ApproveProjectBriefCommand(projectId, edits), ct);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves matching vendors for a project based on brief skills and pgvector cosine similarity.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="maxResults">Maximum number of vendor matches to return (default: 25).</param>
    /// <param name="minScore">Minimum similarity score threshold (default: 0.75).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Matching vendors retrieved.</response>
    [HttpGet("matching-vendors")]
    [ProducesResponseType(typeof(IReadOnlyList<VendorMatchResultDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<VendorMatchResultDto>>> GetMatchingVendors(
        [FromRoute] Guid projectId,
        [FromQuery] int maxResults = 25,
        [FromQuery] double minScore = 0.75,
        CancellationToken ct = default)
    {
        _logger.LogDebug("Fetching matching vendors for project: {ProjectId}", projectId);

        var result = await _mediator.Send(new GetMatchingVendorsQuery(projectId, maxResults, minScore), ct);
        return Ok(result);
    }
}
