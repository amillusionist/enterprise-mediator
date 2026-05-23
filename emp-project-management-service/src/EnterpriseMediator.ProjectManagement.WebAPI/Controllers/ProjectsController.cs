using EnterpriseMediator.ProjectManagement.Application.Common;
using EnterpriseMediator.ProjectManagement.Application.Features.Milestones.Commands.AddMilestone;
using EnterpriseMediator.ProjectManagement.Application.Features.Milestones.Commands.ApproveMilestone;
using EnterpriseMediator.ProjectManagement.Application.Features.Projects.Commands.ApproveBrief;
using EnterpriseMediator.ProjectManagement.Application.Features.Projects.Commands.AwardProject;
using EnterpriseMediator.ProjectManagement.Application.Features.Projects.Commands.ChangeProjectStatus;
using EnterpriseMediator.ProjectManagement.Application.Features.Projects.Commands.ConfigureFinancials;
using EnterpriseMediator.ProjectManagement.Application.Features.Projects.Commands.CreateProject;
using EnterpriseMediator.ProjectManagement.Application.Features.Projects.Commands.DistributeBrief;
using EnterpriseMediator.ProjectManagement.Application.Features.Projects.Commands.UpdateProjectBrief;
using EnterpriseMediator.ProjectManagement.Application.Features.Projects.Commands.UploadSow;
using EnterpriseMediator.ProjectManagement.Application.Features.Projects.Queries.GetMatchingVendors;
using EnterpriseMediator.ProjectManagement.Application.Features.Projects.Queries.GetProjectById;
using EnterpriseMediator.ProjectManagement.Application.Features.Proposals.Commands.SubmitProposal;
using EnterpriseMediator.ProjectManagement.Application.Features.Proposals.Commands.UpdateProposalAssessment;
using EnterpriseMediator.ProjectManagement.Application.Features.Proposals.Queries.GetProjectProposals;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseMediator.ProjectManagement.WebAPI.Controllers;

/// <summary>
/// Manages projects, SOW documents, proposals, and milestones.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly ISender _sender;

    public ProjectsController(ISender sender)
    {
        _sender = sender;
    }

    // ──────────────────────────────────────────────
    // Project CRUD & Lifecycle
    // ──────────────────────────────────────────────

    /// <summary>
    /// Creates a new project for the specified client.
    /// </summary>
    /// <param name="request">The project creation details.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The identifier of the newly created project.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> CreateProject(
        [FromBody] CreateProjectRequest request,
        CancellationToken ct)
    {
        var result = await _sender.Send(
            new CreateProjectCommand(request.ClientId, request.Name, request.Description), ct);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return CreatedAtAction(nameof(GetProjectById), new { projectId = result.Value }, result.Value);
    }

    /// <summary>
    /// Retrieves a project by its identifier.
    /// </summary>
    /// <param name="projectId">The unique project identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The project detail DTO, or 404 if not found.</returns>
    [HttpGet("{projectId:guid}")]
    [ProducesResponseType(typeof(ProjectDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectDetailDto>> GetProjectById(
        Guid projectId,
        CancellationToken ct)
    {
        var dto = await _sender.Send(new GetProjectByIdQuery(projectId), ct);

        if (dto is null)
        {
            return NotFound();
        }

        return Ok(dto);
    }

    /// <summary>
    /// Uploads a Statement of Work document for a project.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="file">The SOW file (PDF, DOCX, or DOC; max 10 MB).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The identifier of the uploaded SOW document.</returns>
    [HttpPost("{projectId:guid}/sow")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> UploadSow(
        Guid projectId,
        IFormFile file,
        CancellationToken ct)
    {
        await using var stream = file.OpenReadStream();

        var result = await _sender.Send(
            new UploadSowCommand(projectId, stream, file.FileName, file.ContentType), ct);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return CreatedAtAction(nameof(GetProjectById), new { projectId }, result.Value);
    }

    /// <summary>
    /// Updates the project brief with extracted or manually edited SOW data.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="request">The updated brief data.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpPut("{projectId:guid}/brief")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateProjectBrief(
        Guid projectId,
        [FromBody] UpdateProjectBriefRequest request,
        CancellationToken ct)
    {
        try
        {
            await _sender.Send(new UpdateProjectBriefCommand(
                projectId,
                request.ScopeSummary,
                request.Deliverables,
                request.RequiredSkills,
                request.Timeline,
                request.Technologies), ct);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Approves the project brief, locking it for distribution.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpPost("{projectId:guid}/brief/approve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ApproveBrief(
        Guid projectId,
        CancellationToken ct)
    {
        try
        {
            await _sender.Send(new ApproveBriefCommand(projectId), ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Distributes the approved project brief to matched vendors.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpPost("{projectId:guid}/brief/distribute")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DistributeBrief(
        Guid projectId,
        CancellationToken ct)
    {
        try
        {
            await _sender.Send(new DistributeBriefCommand(projectId), ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Awards the project to the vendor behind the specified proposal.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="request">The award details containing the proposal identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpPost("{projectId:guid}/award")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AwardProject(
        Guid projectId,
        [FromBody] AwardProjectRequest request,
        CancellationToken ct)
    {
        var result = await _sender.Send(
            new AwardProjectCommand(projectId, request.ProposalId), ct);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return NoContent();
    }

    /// <summary>
    /// Changes the project status. Valid actions: activate, complete, hold, resume, cancel.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="request">The status change action.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpPost("{projectId:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangeProjectStatus(
        Guid projectId,
        [FromBody] ChangeProjectStatusRequest request,
        CancellationToken ct)
    {
        try
        {
            await _sender.Send(new ChangeProjectStatusCommand(projectId, request.Action), ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Configures financial margins for the project.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="request">The financial configuration containing fixed and/or percentage margins.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpPut("{projectId:guid}/financials")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfigureFinancials(
        Guid projectId,
        [FromBody] ConfigureFinancialsRequest request,
        CancellationToken ct)
    {
        try
        {
            await _sender.Send(
                new ConfigureFinancialsCommand(projectId, request.FixedMargin, request.PercentageMargin), ct);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Returns vendors matching the project brief, ranked by cosine similarity.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="limit">Maximum number of vendor matches to return (default 25).</param>
    /// <param name="minSimilarity">Minimum similarity score threshold (default 0.75).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A ranked list of matching vendors.</returns>
    [HttpGet("{projectId:guid}/matching-vendors")]
    [ProducesResponseType(typeof(IReadOnlyList<Domain.Services.VendorMatch>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<Domain.Services.VendorMatch>>> GetMatchingVendors(
        Guid projectId,
        [FromQuery] int limit = 25,
        [FromQuery] double minSimilarity = 0.75,
        CancellationToken ct = default)
    {
        var vendors = await _sender.Send(
            new GetMatchingVendorsQuery(projectId, limit, minSimilarity), ct);

        return Ok(vendors);
    }

    // ──────────────────────────────────────────────
    // Proposals
    // ──────────────────────────────────────────────

    /// <summary>
    /// Submits a proposal from a vendor for the specified project.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="request">The proposal submission details.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The identifier of the newly created proposal.</returns>
    [HttpPost("{projectId:guid}/proposals")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> SubmitProposal(
        Guid projectId,
        [FromBody] SubmitProposalRequest request,
        CancellationToken ct)
    {
        var result = await _sender.Send(new SubmitProposalCommand(
            projectId,
            request.VendorId,
            request.ProposedCost,
            request.Currency,
            request.Timeline,
            request.KeyPersonnel,
            request.CoverLetter,
            request.ProposalDocumentUrl), ct);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return CreatedAtAction(
            nameof(GetProjectProposals),
            new { projectId },
            result.Value);
    }

    /// <summary>
    /// Updates the assessment score and flag for a proposal.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="proposalId">The proposal identifier.</param>
    /// <param name="request">The assessment update containing score and/or flag.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpPut("{projectId:guid}/proposals/{proposalId:guid}/assessment")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateProposalAssessment(
        Guid projectId,
        Guid proposalId,
        [FromBody] UpdateProposalAssessmentRequest request,
        CancellationToken ct)
    {
        try
        {
            await _sender.Send(
                new UpdateProposalAssessmentCommand(projectId, proposalId, request.Score, request.Flag), ct);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Retrieves all proposals submitted for the specified project.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A list of proposal DTOs.</returns>
    [HttpGet("{projectId:guid}/proposals")]
    [ProducesResponseType(typeof(IReadOnlyList<ProposalDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProposalDto>>> GetProjectProposals(
        Guid projectId,
        CancellationToken ct)
    {
        var proposals = await _sender.Send(new GetProjectProposalsQuery(projectId), ct);
        return Ok(proposals);
    }

    // ──────────────────────────────────────────────
    // Milestones
    // ──────────────────────────────────────────────

    /// <summary>
    /// Adds a new milestone to the project.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="request">The milestone details.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The identifier of the newly created milestone.</returns>
    [HttpPost("{projectId:guid}/milestones")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> AddMilestone(
        Guid projectId,
        [FromBody] AddMilestoneRequest request,
        CancellationToken ct)
    {
        var result = await _sender.Send(new AddMilestoneCommand(
            projectId,
            request.Title,
            request.Description,
            request.Amount,
            request.Currency,
            request.Order,
            request.DueDate), ct);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return CreatedAtAction(
            nameof(GetProjectById),
            new { projectId },
            result.Value);
    }

    /// <summary>
    /// Approves a milestone, triggering downstream financial events.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="milestoneId">The milestone identifier.</param>
    /// <param name="request">The approval details containing the approver's contact identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpPost("{projectId:guid}/milestones/{milestoneId:guid}/approve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ApproveMilestone(
        Guid projectId,
        Guid milestoneId,
        [FromBody] ApproveMilestoneRequest request,
        CancellationToken ct)
    {
        try
        {
            await _sender.Send(
                new ApproveMilestoneCommand(projectId, milestoneId, request.ApprovedByContactId), ct);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}

// ──────────────────────────────────────────────
// Request DTOs
// ──────────────────────────────────────────────

/// <summary>Request body for creating a new project.</summary>
public sealed record CreateProjectRequest(Guid ClientId, string Name, string Description);

/// <summary>Request body for updating a project brief.</summary>
public sealed record UpdateProjectBriefRequest(
    string ScopeSummary,
    List<string> Deliverables,
    List<string> RequiredSkills,
    string Timeline,
    List<string> Technologies);

/// <summary>Request body for awarding a project to a proposal.</summary>
public sealed record AwardProjectRequest(Guid ProposalId);

/// <summary>Request body for changing a project's status.</summary>
public sealed record ChangeProjectStatusRequest(string Action);

/// <summary>Request body for configuring project financial margins.</summary>
public sealed record ConfigureFinancialsRequest(decimal? FixedMargin, decimal? PercentageMargin);

/// <summary>Request body for submitting a vendor proposal.</summary>
public sealed record SubmitProposalRequest(
    Guid VendorId,
    decimal ProposedCost,
    string Currency,
    string Timeline,
    string KeyPersonnel,
    string CoverLetter,
    string? ProposalDocumentUrl);

/// <summary>Request body for updating a proposal assessment.</summary>
public sealed record UpdateProposalAssessmentRequest(int? Score, string? Flag);

/// <summary>Request body for adding a milestone.</summary>
public sealed record AddMilestoneRequest(
    string Title,
    string? Description,
    decimal Amount,
    string Currency,
    int Order,
    DateTime? DueDate);

/// <summary>Request body for approving a milestone.</summary>
public sealed record ApproveMilestoneRequest(Guid ApprovedByContactId);
