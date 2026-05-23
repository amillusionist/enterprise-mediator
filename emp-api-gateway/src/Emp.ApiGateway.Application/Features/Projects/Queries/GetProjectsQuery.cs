using EnterpriseMediator.Contracts.Common;
using EnterpriseMediator.Contracts.DTOs.Projects;
using MediatR;

namespace Emp.ApiGateway.Application.Features.Projects.Queries;

/// <summary>
/// Query to retrieve a paginated list of projects with optional filters.
/// </summary>
public record GetProjectsQuery(int Page, int PageSize, string? Search, string? Status, Guid? ClientId) : IRequest<PagedResultDto<ProjectDto>>;
