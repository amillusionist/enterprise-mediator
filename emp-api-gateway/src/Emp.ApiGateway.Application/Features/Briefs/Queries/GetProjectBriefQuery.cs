using EnterpriseMediator.Contracts.DTOs.Projects;
using MediatR;

namespace Emp.ApiGateway.Application.Features.Briefs.Queries;

public record GetProjectBriefQuery(Guid ProjectId) : IRequest<ProjectBriefDto?>;
