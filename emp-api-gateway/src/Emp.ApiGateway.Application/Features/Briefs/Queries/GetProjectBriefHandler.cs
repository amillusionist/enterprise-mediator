using Emp.ApiGateway.Application.Interfaces.Infrastructure;
using EnterpriseMediator.Contracts.DTOs.Projects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Emp.ApiGateway.Application.Features.Briefs.Queries;

public class GetProjectBriefHandler : IRequestHandler<GetProjectBriefQuery, ProjectBriefDto?>
{
    private readonly IProjectServiceClient _projectService;
    private readonly ILogger<GetProjectBriefHandler> _logger;

    public GetProjectBriefHandler(IProjectServiceClient projectService, ILogger<GetProjectBriefHandler> logger)
    {
        _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ProjectBriefDto?> Handle(GetProjectBriefQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching project brief for ProjectId: {ProjectId}", request.ProjectId);
        return await _projectService.GetProjectBriefAsync(request.ProjectId, cancellationToken);
    }
}
