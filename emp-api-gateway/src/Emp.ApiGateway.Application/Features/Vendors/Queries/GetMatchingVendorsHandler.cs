using Emp.ApiGateway.Application.Interfaces.Infrastructure;
using EnterpriseMediator.Contracts.DTOs.Projects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Emp.ApiGateway.Application.Features.Vendors.Queries;

public class GetMatchingVendorsHandler : IRequestHandler<GetMatchingVendorsQuery, IReadOnlyList<VendorMatchResultDto>>
{
    private readonly IProjectServiceClient _projectService;
    private readonly ILogger<GetMatchingVendorsHandler> _logger;

    public GetMatchingVendorsHandler(IProjectServiceClient projectService, ILogger<GetMatchingVendorsHandler> logger)
    {
        _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IReadOnlyList<VendorMatchResultDto>> Handle(GetMatchingVendorsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching matching vendors for ProjectId: {ProjectId}", request.ProjectId);
        return await _projectService.GetMatchingVendorsAsync(request.ProjectId, request.MaxResults, request.MinScore, cancellationToken);
    }
}
