using Emp.ApiGateway.Application.Interfaces.Infrastructure;
using EnterpriseMediator.Contracts.Common;
using EnterpriseMediator.Contracts.DTOs.Projects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Emp.ApiGateway.Application.Features.Projects.Queries;

/// <summary>
/// Handles the GetProjectsQuery by forwarding the request to the Project Microservice.
/// </summary>
public class GetProjectsHandler : IRequestHandler<GetProjectsQuery, PagedResultDto<ProjectDto>>
{
    private readonly IProjectServiceClient _projectService;
    private readonly ILogger<GetProjectsHandler> _logger;

    public GetProjectsHandler(IProjectServiceClient projectService, ILogger<GetProjectsHandler> logger)
    {
        _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PagedResultDto<ProjectDto>> Handle(GetProjectsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching projects list. Page: {Page}, PageSize: {PageSize}, Search: {Search}, Status: {Status}",
            request.Page, request.PageSize, request.Search, request.Status);

        return await _projectService.GetProjectsAsync(
            request.Page, request.PageSize, request.Search, request.Status, request.ClientId, cancellationToken);
    }
}
