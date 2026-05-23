using Emp.ApiGateway.Application.Interfaces.Infrastructure;
using EnterpriseMediator.Contracts.DTOs.Projects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Emp.ApiGateway.Application.Features.Milestones.Queries;

public class GetProjectMilestonesHandler : IRequestHandler<GetProjectMilestonesQuery, IReadOnlyList<MilestoneDto>>
{
    private readonly IProjectServiceClient _projectService;
    private readonly ILogger<GetProjectMilestonesHandler> _logger;

    public GetProjectMilestonesHandler(IProjectServiceClient projectService, ILogger<GetProjectMilestonesHandler> logger)
    {
        _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IReadOnlyList<MilestoneDto>> Handle(GetProjectMilestonesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching milestones for ProjectId: {ProjectId}", request.ProjectId);
        return await _projectService.GetProjectMilestonesAsync(request.ProjectId, cancellationToken);
    }
}
