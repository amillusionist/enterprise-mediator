using EnterpriseMediator.ProjectManagement.Application.Common;
using EnterpriseMediator.ProjectManagement.Domain.Aggregates.ProjectAggregate;
using EnterpriseMediator.ProjectManagement.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EnterpriseMediator.ProjectManagement.Application.Features.Projects.Commands.CreateProject;

public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, Result<Guid>>
{
    private readonly IProjectRepository _projectRepository;
    private readonly ILogger<CreateProjectCommandHandler> _logger;

    public CreateProjectCommandHandler(IProjectRepository projectRepository, ILogger<CreateProjectCommandHandler> logger)
    {
        _projectRepository = projectRepository;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating project {ProjectName} for client {ClientId}", request.Name, request.ClientId);

        if (await _projectRepository.ExistsByNameAsync(request.Name, ct: cancellationToken))
            return Result<Guid>.Failure("A project with this name already exists.");

        var project = Project.Create(request.ClientId, request.Name, request.Description);
        await _projectRepository.AddAsync(project, cancellationToken);
        await _projectRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Project {ProjectId} created successfully", project.Id);
        return Result<Guid>.Success(project.Id);
    }
}
