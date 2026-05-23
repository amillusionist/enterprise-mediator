using EnterpriseMediator.ProjectManagement.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EnterpriseMediator.ProjectManagement.Application.Features.Projects.Commands.ApproveBrief;

public class ApproveBriefCommandHandler : IRequestHandler<ApproveBriefCommand>
{
    private readonly IProjectRepository _projectRepository;
    private readonly ILogger<ApproveBriefCommandHandler> _logger;

    public ApproveBriefCommandHandler(IProjectRepository projectRepository, ILogger<ApproveBriefCommandHandler> logger)
    {
        _projectRepository = projectRepository;
        _logger = logger;
    }

    public async Task Handle(ApproveBriefCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Approving brief for project {ProjectId}", request.ProjectId);

        var project = await _projectRepository.GetByIdAsync(request.ProjectId, cancellationToken)
            ?? throw new KeyNotFoundException($"Project {request.ProjectId} not found.");

        project.ApproveBrief();
        _projectRepository.Update(project);
        await _projectRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Brief approved for project {ProjectId}", request.ProjectId);
    }
}
