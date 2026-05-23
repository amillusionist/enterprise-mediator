using Emp.ApiGateway.Application.Interfaces.Infrastructure;
using EnterpriseMediator.Contracts.DTOs.Projects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Emp.ApiGateway.Application.Features.Projects.Queries;

/// <summary>
/// Handles the GetSowStatusQuery by forwarding the request to the Project Microservice.
/// </summary>
public class GetSowStatusHandler : IRequestHandler<GetSowStatusQuery, SowDocumentDto?>
{
    private readonly IProjectServiceClient _projectService;
    private readonly ILogger<GetSowStatusHandler> _logger;

    public GetSowStatusHandler(IProjectServiceClient projectService, ILogger<GetSowStatusHandler> logger)
    {
        _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<SowDocumentDto?> Handle(GetSowStatusQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching SOW status for ProjectId: {ProjectId}", request.ProjectId);
        return await _projectService.GetSowStatusAsync(request.ProjectId, cancellationToken);
    }
}
