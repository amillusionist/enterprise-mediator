using Emp.ApiGateway.Application.Interfaces.Infrastructure;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Emp.ApiGateway.Application.Features.Projects.Commands;

/// <summary>
/// Handles the UpdateProjectStatusCommand by forwarding the request to the Project Microservice.
/// </summary>
public class UpdateProjectStatusHandler : IRequestHandler<UpdateProjectStatusCommand>
{
    private readonly IProjectServiceClient _projectService;
    private readonly ILogger<UpdateProjectStatusHandler> _logger;

    public UpdateProjectStatusHandler(IProjectServiceClient projectService, ILogger<UpdateProjectStatusHandler> logger)
    {
        _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(UpdateProjectStatusCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating project status. ProjectId: {ProjectId}, NewStatus: {Status}", request.ProjectId, request.Status);
        await _projectService.UpdateProjectStatusAsync(request.ProjectId, request.Status, cancellationToken);
    }
}
