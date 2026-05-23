using System.Net.Mime;
using Emp.ApiGateway.Application.Features.Projects.Commands;
using Emp.ApiGateway.Application.Features.Projects.Queries;
using EnterpriseMediator.Contracts.Common;
using EnterpriseMediator.Contracts.DTOs.Projects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Emp.ApiGateway.Web.Controllers;

/// <summary>
/// Public API Controller for Project management operations.
/// Acts as a BFF entry point orchestrating calls to the underlying Project Microservice.
/// </summary>
[ApiController]
[Route("api/v1/projects")]
[Authorize]
[Produces(MediaTypeNames.Application.Json)]
public class ProjectsController : ControllerBase
{
    private readonly ISender _mediator;
    private readonly ILogger<ProjectsController> _logger;

    public ProjectsController(ISender mediator, ILogger<ProjectsController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves a paginated list of projects with optional filters.
    /// </summary>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="search">Optional search term for project name.</param>
    /// <param name="status">Optional project status filter.</param>
    /// <param name="clientId">Optional client ID filter.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Paginated list of projects.</returns>
    /// <response code="200">Projects retrieved successfully.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<ProjectDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResultDto<ProjectDto>>> GetProjects(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        [FromQuery] Guid? clientId = null,
        CancellationToken ct = default)
    {
        var query = new GetProjectsQuery(page, pageSize, search, status, clientId);
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a specific project by its unique identifier.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The project details.</returns>
    /// <response code="200">Project retrieved successfully.</response>
    /// <response code="404">Project not found.</response>
    [HttpGet("{projectId:guid}")]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectDto>> GetProjectById(
        [FromRoute] Guid projectId,
        CancellationToken ct)
    {
        var query = new GetProjectByIdQuery(projectId);
        var result = await _mediator.Send(query, ct);

        if (result == null)
        {
            _logger.LogWarning("Project not found for ID: {ProjectId}", projectId);
            return NotFound($"Project with ID {projectId} not found.");
        }

        return Ok(result);
    }

    /// <summary>
    /// Creates a new project in the system.
    /// </summary>
    /// <param name="command">The project creation details.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The unique identifier of the created project.</returns>
    /// <response code="201">Project successfully created.</response>
    /// <response code="400">Invalid request data.</response>
    /// <response code="401">Unauthorized access.</response>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Guid>> CreateProject(
        [FromBody] CreateProjectCommand command,
        CancellationToken ct)
    {
        _logger.LogInformation("Received request to create project: {ProjectName}", command.Name);

        var projectId = await _mediator.Send(command, ct);

        _logger.LogInformation("Project created successfully with ID: {ProjectId}", projectId);

        return CreatedAtAction(nameof(GetProjectById), new { projectId }, projectId);
    }

    /// <summary>
    /// Updates the status of a project.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="request">The new status.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Project status updated successfully.</response>
    /// <response code="400">Invalid status transition.</response>
    [HttpPatch("{projectId:guid}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateProjectStatus(
        [FromRoute] Guid projectId,
        [FromBody] UpdateProjectStatusRequest request,
        CancellationToken ct)
    {
        _logger.LogInformation("Updating project status. ProjectId: {ProjectId}, NewStatus: {Status}", projectId, request.Status);
        await _mediator.Send(new UpdateProjectStatusCommand(projectId, request.Status), ct);
        return Ok(new { message = "Project status updated." });
    }

    /// <summary>
    /// Retrieves the aggregated dashboard view for a specific project.
    /// This is a BFF-pattern endpoint that aggregates data from Project and Financial services.
    /// </summary>
    /// <param name="projectId">The unique identifier of the project.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Aggregated project dashboard data.</returns>
    /// <response code="200">Returns the dashboard data.</response>
    /// <response code="404">Project not found.</response>
    [HttpGet("{projectId:guid}/dashboard")]
    [ProducesResponseType(typeof(ProjectDashboardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectDashboardResponse>> GetProjectDashboard(
        [FromRoute] Guid projectId,
        CancellationToken ct)
    {
        _logger.LogDebug("Retrieving dashboard for project: {ProjectId}", projectId);

        var query = new GetProjectDashboardQuery(projectId);
        var result = await _mediator.Send(query, ct);

        if (result == null)
        {
            _logger.LogWarning("Project dashboard not found for ID: {ProjectId}", projectId);
            return NotFound($"Project with ID {projectId} not found.");
        }

        return Ok(result);
    }

    /// <summary>
    /// Uploads a Statement of Work (SOW) document for a project.
    /// The file is streamed to storage and triggers an asynchronous AI processing workflow.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="file">The SOW document (PDF or DOCX).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Accepted status indicating processing has started.</returns>
    /// <response code="202">File accepted for processing.</response>
    /// <response code="400">Invalid file format or size.</response>
    [HttpPost("{projectId:guid}/sow")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadSow(
        [FromRoute] Guid projectId,
        IFormFile file,
        CancellationToken ct)
    {
        if (file == null || file.Length == 0)
        {
            _logger.LogWarning("Upload SOW attempt with empty file for project {ProjectId}", projectId);
            return BadRequest("No file uploaded.");
        }

        const long maxFileSize = 10 * 1024 * 1024;
        if (file.Length > maxFileSize)
        {
            return BadRequest("File size exceeds the maximum allowed limit of 10MB.");
        }

        var allowedExtensions = new[] { ".pdf", ".docx", ".doc" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
        {
            return BadRequest("Invalid file type. Only PDF and Word documents are allowed.");
        }

        _logger.LogInformation(
            "Uploading SOW for project {ProjectId}. FileName: {FileName}, Size: {Size}",
            projectId,
            file.FileName,
            file.Length);

        using var stream = file.OpenReadStream();

        var command = new UploadSowCommand(projectId, stream, file.FileName, file.ContentType);
        await _mediator.Send(command, ct);

        return Accepted(new { message = "SOW uploaded successfully and queued for processing." });
    }

    /// <summary>
    /// Retrieves the SOW processing status for a project.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>SOW document status.</returns>
    /// <response code="200">SOW status retrieved.</response>
    /// <response code="404">SOW not found for this project.</response>
    [HttpGet("{projectId:guid}/sow/status")]
    [ProducesResponseType(typeof(SowDocumentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SowDocumentDto>> GetSowStatus(
        [FromRoute] Guid projectId,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new GetSowStatusQuery(projectId), ct);

        if (result == null)
        {
            return NotFound($"SOW not found for project {projectId}.");
        }

        return Ok(result);
    }

    /// <summary>
    /// Updates the project brief with admin edits.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="request">The brief update data.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Brief updated successfully.</response>
    [HttpPut("{projectId:guid}/brief")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateBrief(
        [FromRoute] Guid projectId,
        [FromBody] UpdateProjectBriefRequest request,
        CancellationToken ct)
    {
        _logger.LogInformation("Updating brief for project: {ProjectId}", projectId);
        await _mediator.Send(new UpdateBriefCommand(projectId, request), ct);
        return Ok(new { message = "Brief updated." });
    }

    /// <summary>
    /// Distributes the approved brief to selected vendors for proposal submission.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="request">The vendor IDs to distribute to.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Brief distributed to vendors.</response>
    [HttpPost("{projectId:guid}/distribute")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DistributeBrief(
        [FromRoute] Guid projectId,
        [FromBody] DistributeBriefRequest request,
        CancellationToken ct)
    {
        _logger.LogInformation("Distributing brief for project {ProjectId} to {VendorCount} vendors",
            projectId, request.VendorIds.Length);
        await _mediator.Send(new DistributeBriefCommand(projectId, request.VendorIds), ct);
        return Ok(new { message = "Brief distributed to vendors." });
    }

    /// <summary>
    /// Awards a project to a specific vendor.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="request">The vendor to award the project to.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Project awarded successfully.</response>
    [HttpPost("{projectId:guid}/award")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AwardProject(
        [FromRoute] Guid projectId,
        [FromBody] AwardProjectRequest request,
        CancellationToken ct)
    {
        _logger.LogInformation("Awarding project {ProjectId} to vendor {VendorId}", projectId, request.VendorId);
        await _mediator.Send(new AwardProjectCommand(projectId, request.VendorId), ct);
        return Ok(new { message = "Project awarded." });
    }
}

/// <summary>
/// Request to update a project's status.
/// </summary>
public record UpdateProjectStatusRequest(string Status);

/// <summary>
/// Request to distribute a brief to selected vendors.
/// </summary>
public record DistributeBriefRequest(Guid[] VendorIds);

/// <summary>
/// Request to award a project to a vendor.
/// </summary>
public record AwardProjectRequest(Guid VendorId);
