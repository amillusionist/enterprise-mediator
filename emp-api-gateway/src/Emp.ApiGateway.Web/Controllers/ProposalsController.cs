using System.Net.Mime;
using Emp.ApiGateway.Application.Interfaces.Infrastructure;
using EnterpriseMediator.Contracts.DTOs.Projects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Emp.ApiGateway.Web.Controllers;

/// <summary>
/// Manages vendor proposals for projects.
/// Handles authenticated proposal management and public vendor portal endpoints.
/// </summary>
[ApiController]
[Produces(MediaTypeNames.Application.Json)]
public class ProposalsController : ControllerBase
{
    private readonly IProjectServiceClient _projectService;
    private readonly ILogger<ProposalsController> _logger;

    public ProposalsController(IProjectServiceClient projectService, ILogger<ProposalsController> logger)
    {
        _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves all proposals for a specific project.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of proposals for the project.</returns>
    /// <response code="200">Proposals retrieved successfully.</response>
    [HttpGet("api/v1/projects/{projectId:guid}/proposals")]
    [Authorize]
    [ProducesResponseType(typeof(IReadOnlyList<ProposalDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProposalDto>>> GetProjectProposals(
        [FromRoute] Guid projectId,
        CancellationToken ct)
    {
        _logger.LogDebug("Retrieving proposals for project: {ProjectId}", projectId);
        var result = await _projectService.GetProjectProposalsAsync(projectId, ct);
        return Ok(result);
    }

    /// <summary>
    /// Updates the status of a proposal (e.g., shortlist, reject).
    /// </summary>
    /// <param name="proposalId">The proposal identifier.</param>
    /// <param name="request">New status and optional notes.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Proposal status updated.</response>
    [HttpPut("api/v1/proposals/{proposalId:guid}/status")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateProposalStatus(
        [FromRoute] Guid proposalId,
        [FromBody] UpdateProposalStatusRequest request,
        CancellationToken ct)
    {
        _logger.LogInformation("Updating proposal status. ProposalId: {ProposalId}, Status: {Status}", proposalId, request.Status);
        await _projectService.UpdateProposalStatusAsync(proposalId, request.Status, request.Notes, ct);
        return Ok(new { message = "Proposal status updated." });
    }

    /// <summary>
    /// Awards a proposal, triggering the project award workflow.
    /// </summary>
    /// <param name="proposalId">The proposal identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Proposal awarded.</response>
    [HttpPost("api/v1/proposals/{proposalId:guid}/award")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> AwardProposal(
        [FromRoute] Guid proposalId,
        CancellationToken ct)
    {
        _logger.LogInformation("Awarding proposal: {ProposalId}", proposalId);
        await _projectService.AwardProposalAsync(proposalId, ct);
        return Ok(new { message = "Proposal awarded." });
    }

    /// <summary>
    /// Public endpoint for vendors to submit proposals via a secure token link.
    /// Accepts multipart form data with an optional file attachment.
    /// </summary>
    /// <param name="token">The secure vendor portal token.</param>
    /// <param name="form">Proposal submission data with optional file.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Proposal submitted successfully.</response>
    /// <response code="400">Invalid submission data.</response>
    [HttpPost("api/v1/public/proposals/{token}/submit")]
    [AllowAnonymous]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SubmitProposal(
        [FromRoute] string token,
        [FromForm] ProposalSubmissionForm form,
        CancellationToken ct)
    {
        _logger.LogInformation("Public proposal submission received via portal token");

        Stream? fileStream = null;
        string? fileName = null;
        if (form.File != null)
        {
            fileStream = form.File.OpenReadStream();
            fileName = form.File.FileName;
        }

        try
        {
            await _projectService.SubmitProposalAsync(
                token, fileStream, fileName, form.Cost, form.Timeline, form.KeyPersonnel, ct);
            return Ok(new { message = "Proposal submitted successfully." });
        }
        finally
        {
            if (fileStream != null)
            {
                await fileStream.DisposeAsync();
            }
        }
    }

    /// <summary>
    /// Public endpoint to retrieve the project brief via a vendor portal token.
    /// Used by vendors to review brief details before submitting a proposal.
    /// </summary>
    /// <param name="token">The secure vendor portal token.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The project brief associated with the token.</returns>
    /// <response code="200">Brief retrieved successfully.</response>
    /// <response code="404">Token not found or expired.</response>
    [HttpGet("api/v1/public/proposals/portal/{token}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ProjectBriefDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectBriefDto>> GetPortalBrief(
        [FromRoute] string token,
        CancellationToken ct)
    {
        var result = await _projectService.GetPortalBriefAsync(token, ct);

        if (result == null)
        {
            return NotFound("Portal token not found or expired.");
        }

        return Ok(result);
    }
}

/// <summary>
/// Request payload for updating a proposal's status.
/// </summary>
public record UpdateProposalStatusRequest(string Status, string? Notes);

/// <summary>
/// Form data for public proposal submission via vendor portal.
/// </summary>
public record ProposalSubmissionForm
{
    /// <summary>
    /// Proposed cost for the project.
    /// </summary>
    public decimal Cost { get; init; }

    /// <summary>
    /// Proposed timeline (e.g., "8 weeks").
    /// </summary>
    public string Timeline { get; init; } = string.Empty;

    /// <summary>
    /// Key personnel to be assigned to the project.
    /// </summary>
    public string KeyPersonnel { get; init; } = string.Empty;

    /// <summary>
    /// Optional proposal document attachment.
    /// </summary>
    public IFormFile? File { get; init; }
}
