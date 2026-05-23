using EnterpriseMediator.ProjectManagement.Application.Common;
using EnterpriseMediator.ProjectManagement.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EnterpriseMediator.ProjectManagement.Application.Features.Projects.Queries.GetProjects;

public class GetProjectsQueryHandler : IRequestHandler<GetProjectsQuery, PagedResult<ProjectListItemDto>>
{
    private readonly IProjectRepository _projectRepository;
    private readonly ILogger<GetProjectsQueryHandler> _logger;

    public GetProjectsQueryHandler(
        IProjectRepository projectRepository,
        ILogger<GetProjectsQueryHandler> logger)
    {
        _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PagedResult<ProjectListItemDto>> Handle(GetProjectsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching projects. Page: {Page}, PageSize: {PageSize}, Search: {Search}, Status: {Status}, ClientId: {ClientId}",
            request.Page, request.PageSize, request.Search, request.Status, request.ClientId);

        var (projects, totalCount) = await _projectRepository.GetPagedAsync(
            request.Page,
            request.PageSize,
            request.Search,
            request.Status,
            request.ClientId,
            cancellationToken);

        var items = projects.Select(p => new ProjectListItemDto(
            p.Id,
            p.ClientId,
            p.Name,
            p.Status.ToString(),
            p.CreatedAt,
            p.Proposals.Count,
            p.Milestones.Count,
            p.AwardedVendorId)).ToList();

        return new PagedResult<ProjectListItemDto>(items, totalCount, request.Page, request.PageSize);
    }
}
