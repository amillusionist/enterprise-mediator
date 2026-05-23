using EnterpriseMediator.ProjectManagement.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EnterpriseMediator.ProjectManagement.Application.Features.Projects.Commands.DistributeBrief;

public class DistributeBriefCommandHandler : IRequestHandler<DistributeBriefCommand>
{
    private readonly IProjectRepository _projectRepository;
    private readonly ILogger<DistributeBriefCommandHandler> _logger;

    public DistributeBriefCommandHandler(IProjectRepository projectRepository, ILogger<DistributeBriefCommandHandler> logger)
    {
        _projectRepository = projectRepository;
        _logger = logger;
    }

    public async Task Handle(DistributeBriefCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Distributing brief for project {ProjectId}", request.ProjectId);

        var project = await _projectRepository.GetByIdAsync(request.ProjectId, cancellationToken)
            ?? throw new KeyNotFoundException($"Project {request.ProjectId} not found.");

        project.DistributeBrief();
        _projectRepository.Update(project);
        await _projectRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Brief distributed for project {ProjectId}", request.ProjectId);
    }
}
