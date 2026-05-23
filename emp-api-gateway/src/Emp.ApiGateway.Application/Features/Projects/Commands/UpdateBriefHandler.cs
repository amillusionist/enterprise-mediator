using Emp.ApiGateway.Application.Interfaces.Infrastructure;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Emp.ApiGateway.Application.Features.Projects.Commands;

/// <summary>
/// Handles the UpdateBriefCommand by forwarding the request to the Project Microservice.
/// </summary>
public class UpdateBriefHandler : IRequestHandler<UpdateBriefCommand>
{
    private readonly IProjectServiceClient _projectService;
    private readonly ILogger<UpdateBriefHandler> _logger;

    public UpdateBriefHandler(IProjectServiceClient projectService, ILogger<UpdateBriefHandler> logger)
    {
        _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(UpdateBriefCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating brief for ProjectId: {ProjectId}", request.ProjectId);
        await _projectService.UpdateBriefAsync(request.ProjectId, request.Request, cancellationToken);
    }
}
