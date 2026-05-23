using Emp.ApiGateway.Application.Interfaces.Infrastructure;
using EnterpriseMediator.Contracts.DTOs.Projects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Emp.ApiGateway.Application.Features.Projects.Queries;

/// <summary>
/// Handles the GetProjectByIdQuery by forwarding the request to the Project Microservice.
/// </summary>
public class GetProjectByIdHandler : IRequestHandler<GetProjectByIdQuery, ProjectDto?>
{
    private readonly IProjectServiceClient _projectService;
    private readonly ILogger<GetProjectByIdHandler> _logger;

    public GetProjectByIdHandler(IProjectServiceClient projectService, ILogger<GetProjectByIdHandler> logger)
    {
        _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ProjectDto?> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching project by ID: {ProjectId}", request.ProjectId);
        return await _projectService.GetProjectByIdAsync(request.ProjectId, cancellationToken);
    }
}
