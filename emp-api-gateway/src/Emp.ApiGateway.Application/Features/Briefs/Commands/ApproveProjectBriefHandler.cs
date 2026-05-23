using Emp.ApiGateway.Application.Interfaces.Infrastructure;
using EnterpriseMediator.Contracts.DTOs.Projects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Emp.ApiGateway.Application.Features.Briefs.Commands;

public class ApproveProjectBriefHandler : IRequestHandler<ApproveProjectBriefCommand, ProjectBriefDto>
{
    private readonly IProjectServiceClient _projectService;
    private readonly ILogger<ApproveProjectBriefHandler> _logger;

    public ApproveProjectBriefHandler(IProjectServiceClient projectService, ILogger<ApproveProjectBriefHandler> logger)
    {
        _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ProjectBriefDto> Handle(ApproveProjectBriefCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Approving project brief for ProjectId: {ProjectId}", request.ProjectId);
        return await _projectService.ApproveProjectBriefAsync(request.ProjectId, request.Edits, cancellationToken);
    }
}
