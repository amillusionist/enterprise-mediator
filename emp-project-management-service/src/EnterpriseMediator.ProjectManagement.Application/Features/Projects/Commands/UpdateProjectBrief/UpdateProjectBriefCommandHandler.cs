using EnterpriseMediator.ProjectManagement.Domain.Aggregates.ProjectAggregate;
using EnterpriseMediator.ProjectManagement.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EnterpriseMediator.ProjectManagement.Application.Features.Projects.Commands.UpdateProjectBrief;

public class UpdateProjectBriefCommandHandler : IRequestHandler<UpdateProjectBriefCommand>
{
    private readonly IProjectRepository _projectRepository;
    private readonly ILogger<UpdateProjectBriefCommandHandler> _logger;

    public UpdateProjectBriefCommandHandler(IProjectRepository projectRepository, ILogger<UpdateProjectBriefCommandHandler> logger)
    {
        _projectRepository = projectRepository;
        _logger = logger;
    }

    public async Task Handle(UpdateProjectBriefCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating brief for project {ProjectId}", request.ProjectId);

        var project = await _projectRepository.GetByIdAsync(request.ProjectId, cancellationToken)
            ?? throw new KeyNotFoundException($"Project {request.ProjectId} not found.");

        var sowDetails = new SowDetails(
            request.ScopeSummary,
            request.Deliverables,
            request.RequiredSkills,
            request.Technologies,
            request.Timeline);

        project.UpdateBrief(sowDetails);
        _projectRepository.Update(project);
        await _projectRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Brief updated for project {ProjectId}", request.ProjectId);
    }
}
