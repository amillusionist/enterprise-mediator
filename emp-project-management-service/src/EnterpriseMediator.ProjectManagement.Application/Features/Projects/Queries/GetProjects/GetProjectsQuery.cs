using EnterpriseMediator.ProjectManagement.Application.Common;
using MediatR;

namespace EnterpriseMediator.ProjectManagement.Application.Features.Projects.Queries.GetProjects;

public record GetProjectsQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    string? Status = null,
    Guid? ClientId = null) : IRequest<PagedResult<ProjectListItemDto>>;

public record ProjectListItemDto(
    Guid Id,
    Guid ClientId,
    string Name,
    string Status,
    DateTime CreatedAt,
    int ProposalCount,
    int MilestoneCount,
    Guid? AwardedVendorId);
