using EnterpriseMediator.ProjectManagement.Domain.Interfaces;
using MediatR;

namespace EnterpriseMediator.ProjectManagement.Application.Features.Projects.Queries.GetProjectById;

public class GetProjectByIdQueryHandler : IRequestHandler<GetProjectByIdQuery, ProjectDetailDto?>
{
    private readonly IProjectRepository _projectRepository;

    public GetProjectByIdQueryHandler(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<ProjectDetailDto?> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdFullAsync(request.ProjectId, cancellationToken);
        if (project == null) return null;

        SowDetailsDto? sowDto = null;
        if (project.SowDetails != null)
        {
            sowDto = new SowDetailsDto(
                project.SowDetails.ScopeSummary,
                project.SowDetails.Deliverables,
                project.SowDetails.RequiredSkills,
                project.SowDetails.Technologies,
                project.SowDetails.EstimationTimeline);
        }

        return new ProjectDetailDto(
            project.Id,
            project.ClientId,
            project.Name,
            project.Description,
            project.Status.ToString(),
            project.CreatedAt,
            project.CompletedAt,
            sowDto,
            project.AwardedVendorId,
            project.FixedMargin,
            project.PercentageMargin,
            project.Proposals.Count,
            project.Milestones.Count);
    }
}
