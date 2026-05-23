using EnterpriseMediator.ProjectManagement.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EnterpriseMediator.ProjectManagement.Application.Features.Projects.Commands.ConfigureFinancials;

public class ConfigureFinancialsCommandHandler : IRequestHandler<ConfigureFinancialsCommand>
{
    private readonly IProjectRepository _projectRepository;
    private readonly ILogger<ConfigureFinancialsCommandHandler> _logger;

    public ConfigureFinancialsCommandHandler(IProjectRepository projectRepository, ILogger<ConfigureFinancialsCommandHandler> logger)
    {
        _projectRepository = projectRepository;
        _logger = logger;
    }

    public async Task Handle(ConfigureFinancialsCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Configuring financials for project {ProjectId}", request.ProjectId);

        var project = await _projectRepository.GetByIdAsync(request.ProjectId, cancellationToken)
            ?? throw new KeyNotFoundException($"Project {request.ProjectId} not found.");

        project.ConfigureFinancials(request.FixedMargin, request.PercentageMargin);
        _projectRepository.Update(project);
        await _projectRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
    }
}
