using Emp.ApiGateway.Application.Interfaces.Infrastructure;
using EnterpriseMediator.Contracts.DTOs.Projects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Emp.ApiGateway.Application.Features.Milestones.Commands;

public class ApproveMilestoneHandler : IRequestHandler<ApproveMilestoneCommand, MilestoneDto>
{
    private readonly IProjectServiceClient _projectService;
    private readonly ILogger<ApproveMilestoneHandler> _logger;

    public ApproveMilestoneHandler(IProjectServiceClient projectService, ILogger<ApproveMilestoneHandler> logger)
    {
        _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<MilestoneDto> Handle(ApproveMilestoneCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Approving milestone: {MilestoneId}", request.MilestoneId);
        return await _projectService.ApproveMilestoneAsync(request.MilestoneId, cancellationToken);
    }
}
