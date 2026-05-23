using EnterpriseMediator.ProjectManagement.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EnterpriseMediator.ProjectManagement.Application.Features.Projects.Commands.ChangeProjectStatus;

public class ChangeProjectStatusCommandHandler : IRequestHandler<ChangeProjectStatusCommand>
{
    private readonly IProjectRepository _projectRepository;
    private readonly ILogger<ChangeProjectStatusCommandHandler> _logger;

    public ChangeProjectStatusCommandHandler(IProjectRepository projectRepository, ILogger<ChangeProjectStatusCommandHandler> logger)
    {
        _projectRepository = projectRepository;
        _logger = logger;
    }

    public async Task Handle(ChangeProjectStatusCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Changing status of project {ProjectId} with action {Action}", request.ProjectId, request.Action);

        var project = await _projectRepository.GetByIdAsync(request.ProjectId, cancellationToken)
            ?? throw new KeyNotFoundException($"Project {request.ProjectId} not found.");

        switch (request.Action.ToLowerInvariant())
        {
            case "activate":
                project.Activate();
                break;
            case "complete":
                project.Complete();
                break;
            case "hold":
                project.PutOnHold();
                break;
            case "resume":
                project.Resume();
                break;
            case "cancel":
                project.Cancel();
                break;
            default:
                throw new ArgumentException($"Unknown action: {request.Action}");
        }

        _projectRepository.Update(project);
        await _projectRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Project {ProjectId} status changed via action {Action}", request.ProjectId, request.Action);
    }
}
