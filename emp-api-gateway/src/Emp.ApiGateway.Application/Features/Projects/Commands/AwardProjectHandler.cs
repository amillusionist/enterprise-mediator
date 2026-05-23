using Emp.ApiGateway.Application.Interfaces.Infrastructure;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Emp.ApiGateway.Application.Features.Projects.Commands;

/// <summary>
/// Handles the AwardProjectCommand by forwarding the request to the Project Microservice.
/// </summary>
public class AwardProjectHandler : IRequestHandler<AwardProjectCommand>
{
    private readonly IProjectServiceClient _projectService;
    private readonly ILogger<AwardProjectHandler> _logger;

    public AwardProjectHandler(IProjectServiceClient projectService, ILogger<AwardProjectHandler> logger)
    {
        _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(AwardProjectCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Awarding project {ProjectId} to vendor {VendorId}", request.ProjectId, request.VendorId);
        await _projectService.AwardProjectAsync(request.ProjectId, request.VendorId, cancellationToken);
    }
}
