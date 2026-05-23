using EnterpriseMediator.ProjectManagement.Domain.Interfaces;
using EnterpriseMediator.ProjectManagement.Domain.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EnterpriseMediator.ProjectManagement.Application.Features.Projects.Queries.GetMatchingVendors;

public class GetMatchingVendorsQueryHandler : IRequestHandler<GetMatchingVendorsQuery, IReadOnlyList<VendorMatch>>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IVendorMatchingService _vendorMatchingService;
    private readonly ILogger<GetMatchingVendorsQueryHandler> _logger;

    public GetMatchingVendorsQueryHandler(
        IProjectRepository projectRepository,
        IVendorMatchingService vendorMatchingService,
        ILogger<GetMatchingVendorsQueryHandler> logger)
    {
        _projectRepository = projectRepository;
        _vendorMatchingService = vendorMatchingService;
        _logger = logger;
    }

    public async Task<IReadOnlyList<VendorMatch>> Handle(GetMatchingVendorsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Finding matching vendors for project {ProjectId}", request.ProjectId);

        var project = await _projectRepository.GetByIdAsync(request.ProjectId, cancellationToken)
            ?? throw new KeyNotFoundException($"Project {request.ProjectId} not found.");

        if (project.SowDetails == null || !project.SowDetails.IsPopulated())
            throw new InvalidOperationException("Project does not have processed SOW details for vendor matching.");

        return await _vendorMatchingService.FindMatchingVendorsAsync(
            project.SowDetails, request.Limit, request.MinSimilarity, cancellationToken);
    }
}
