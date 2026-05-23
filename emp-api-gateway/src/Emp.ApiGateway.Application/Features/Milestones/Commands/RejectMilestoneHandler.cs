using Emp.ApiGateway.Application.Interfaces.Infrastructure;
using EnterpriseMediator.Contracts.DTOs.Projects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Emp.ApiGateway.Application.Features.Milestones.Commands;

public class RejectMilestoneHandler : IRequestHandler<RejectMilestoneCommand, MilestoneDto>
{
    private readonly IProjectServiceClient _projectService;
    private readonly ILogger<RejectMilestoneHandler> _logger;

    public RejectMilestoneHandler(IProjectServiceClient projectService, ILogger<RejectMilestoneHandler> logger)
    {
        _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<MilestoneDto> Handle(RejectMilestoneCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Rejecting milestone: {MilestoneId}, Reason: {Reason}", request.MilestoneId, request.Reason);
        return await _projectService.RejectMilestoneAsync(request.MilestoneId, request.Reason, cancellationToken);
    }
}
