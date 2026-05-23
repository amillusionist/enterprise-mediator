using Emp.ApiGateway.Application.Interfaces.Infrastructure;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Emp.ApiGateway.Application.Features.Projects.Commands;

/// <summary>
/// Handles the DistributeBriefCommand by forwarding the request to the Project Microservice.
/// </summary>
public class DistributeBriefHandler : IRequestHandler<DistributeBriefCommand>
{
    private readonly IProjectServiceClient _projectService;
    private readonly ILogger<DistributeBriefHandler> _logger;

    public DistributeBriefHandler(IProjectServiceClient projectService, ILogger<DistributeBriefHandler> logger)
    {
        _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(DistributeBriefCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Distributing brief for ProjectId: {ProjectId} to {VendorCount} vendors",
            request.ProjectId, request.VendorIds.Length);
        await _projectService.DistributeBriefAsync(request.ProjectId, request.VendorIds, cancellationToken);
    }
}
