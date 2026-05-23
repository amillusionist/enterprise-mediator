using EnterpriseMediator.ProjectManagement.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EnterpriseMediator.ProjectManagement.Application.Features.Milestones.Commands.ApproveMilestone;

public class ApproveMilestoneCommandHandler : IRequestHandler<ApproveMilestoneCommand>
{
    private readonly IProjectRepository _projectRepository;
    private readonly ILogger<ApproveMilestoneCommandHandler> _logger;

    public ApproveMilestoneCommandHandler(IProjectRepository projectRepository, ILogger<ApproveMilestoneCommandHandler> logger)
    {
        _projectRepository = projectRepository;
        _logger = logger;
    }

    public async Task Handle(ApproveMilestoneCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Approving milestone {MilestoneId} for project {ProjectId}", request.MilestoneId, request.ProjectId);

        var project = await _projectRepository.GetByIdWithMilestonesAsync(request.ProjectId, cancellationToken)
            ?? throw new KeyNotFoundException($"Project {request.ProjectId} not found.");

        project.ApproveMilestone(request.MilestoneId, request.ApprovedByContactId);
        _projectRepository.Update(project);
        await _projectRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Milestone {MilestoneId} approved for project {ProjectId}", request.MilestoneId, request.ProjectId);
    }
}
