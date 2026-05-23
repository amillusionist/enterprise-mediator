using EnterpriseMediator.Contracts.Common;
using EnterpriseMediator.Contracts.DTOs.Projects;

namespace Emp.ApiGateway.Application.Interfaces.Infrastructure;

/// <summary>
/// Contract for communicating with the downstream Project Microservice.
/// Handles project lifecycle data, document management, briefs, proposals, and vendor matching.
/// </summary>
public interface IProjectServiceClient
{
    /// <summary>
    /// Retrieves detailed information about a specific project from the Project Service.
    /// </summary>
    Task<InternalProjectDto?> GetProjectDetailsAsync(Guid projectId, CancellationToken ct);

    /// <summary>
    /// Retrieves a specific project by ID using the shared ProjectDto contract.
    /// </summary>
    Task<ProjectDto?> GetProjectByIdAsync(Guid projectId, CancellationToken ct);

    /// <summary>
    /// Retrieves a paginated list of projects with optional filters.
    /// </summary>
    Task<PagedResultDto<ProjectDto>> GetProjectsAsync(int page, int pageSize, string? search, string? status, Guid? clientId, CancellationToken ct);

    /// <summary>
    /// Sends a request to the Project Service to create a new project entity.
    /// </summary>
    Task<Guid> CreateProjectAsync(CreateProjectDto dto, CancellationToken ct);

    /// <summary>
    /// Updates the status of a project.
    /// </summary>
    Task UpdateProjectStatusAsync(Guid projectId, string status, CancellationToken ct);

    /// <summary>
    /// Uploads a Statement of Work (SOW) document stream to the Project Service.
    /// </summary>
    Task UploadSowAsync(Guid projectId, Stream fileStream, string fileName, string contentType, CancellationToken ct);

    /// <summary>
    /// Retrieves the SOW processing status for a project.
    /// </summary>
    Task<SowDocumentDto?> GetSowStatusAsync(Guid projectId, CancellationToken ct);

    /// <summary>
    /// Retrieves the AI-extracted project brief for review.
    /// </summary>
    Task<ProjectBriefDto?> GetProjectBriefAsync(Guid projectId, CancellationToken ct);

    /// <summary>
    /// Approves the AI-extracted project brief, optionally with edits.
    /// Triggers vendor matching via ProjectBriefApprovedEvent.
    /// </summary>
    Task<ProjectBriefDto> ApproveProjectBriefAsync(Guid projectId, UpdateProjectBriefRequest? edits, CancellationToken ct);

    /// <summary>
    /// Updates the project brief with admin edits.
    /// </summary>
    Task UpdateBriefAsync(Guid projectId, UpdateProjectBriefRequest request, CancellationToken ct);

    /// <summary>
    /// Distributes the approved brief to selected vendors for proposal submission.
    /// </summary>
    Task DistributeBriefAsync(Guid projectId, Guid[] vendorIds, CancellationToken ct);

    /// <summary>
    /// Awards a project to a specific vendor.
    /// </summary>
    Task AwardProjectAsync(Guid projectId, Guid vendorId, CancellationToken ct);

    /// <summary>
    /// Queries vendor matching results for a project using pgvector cosine similarity.
    /// </summary>
    Task<IReadOnlyList<VendorMatchResultDto>> GetMatchingVendorsAsync(Guid projectId, int maxResults, double minScore, CancellationToken ct);

    /// <summary>
    /// Retrieves milestones for a project.
    /// </summary>
    Task<IReadOnlyList<MilestoneDto>> GetProjectMilestonesAsync(Guid projectId, CancellationToken ct);

    /// <summary>
    /// Retrieves a specific milestone.
    /// </summary>
    Task<MilestoneDto?> GetMilestoneAsync(Guid milestoneId, CancellationToken ct);

    /// <summary>
    /// Approves a milestone (typically by client via secure link).
    /// </summary>
    Task<MilestoneDto> ApproveMilestoneAsync(Guid milestoneId, CancellationToken ct);

    /// <summary>
    /// Rejects a milestone with a reason.
    /// </summary>
    Task<MilestoneDto> RejectMilestoneAsync(Guid milestoneId, string reason, CancellationToken ct);

    /// <summary>
    /// Retrieves all proposals for a project.
    /// </summary>
    Task<IReadOnlyList<ProposalDto>> GetProjectProposalsAsync(Guid projectId, CancellationToken ct);

    /// <summary>
    /// Updates the status of a proposal.
    /// </summary>
    Task UpdateProposalStatusAsync(Guid proposalId, string status, string? notes, CancellationToken ct);

    /// <summary>
    /// Awards a proposal, triggering the project award workflow.
    /// </summary>
    Task AwardProposalAsync(Guid proposalId, CancellationToken ct);

    /// <summary>
    /// Submits a vendor proposal via the public portal.
    /// </summary>
    Task SubmitProposalAsync(string token, Stream? fileStream, string? fileName, decimal cost, string timeline, string keyPersonnel, CancellationToken ct);

    /// <summary>
    /// Retrieves the project brief via a public vendor portal token.
    /// </summary>
    Task<ProjectBriefDto?> GetPortalBriefAsync(string token, CancellationToken ct);
}

/// <summary>
/// Data Transfer Object representing the internal view of a project returned by the microservice.
/// </summary>
public record InternalProjectDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public Guid ClientId { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}

/// <summary>
/// Data Transfer Object for creating a new project.
/// </summary>
public record CreateProjectDto(string Name, string Description, Guid ClientId, DateTime? StartDate, DateTime? EndDate);
