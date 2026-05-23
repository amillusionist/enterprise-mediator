using MediatR;

namespace EnterpriseMediator.ProjectManagement.Application.Features.Projects.Queries.GetProjectById;

public record GetProjectByIdQuery(Guid ProjectId) : IRequest<ProjectDetailDto?>;

public record ProjectDetailDto(
    Guid Id,
    Guid ClientId,
    string Name,
    string Description,
    string Status,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    SowDetailsDto? SowDetails,
    Guid? AwardedVendorId,
    decimal? FixedMargin,
    decimal? PercentageMargin,
    int ProposalCount,
    int MilestoneCount);

public record SowDetailsDto(
    string ScopeSummary,
    IReadOnlyList<string> Deliverables,
    IReadOnlyList<string> RequiredSkills,
    IReadOnlyList<string> Technologies,
    string EstimationTimeline);
