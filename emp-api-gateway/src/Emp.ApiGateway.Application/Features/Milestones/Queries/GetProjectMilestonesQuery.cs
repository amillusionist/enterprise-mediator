using EnterpriseMediator.Contracts.DTOs.Projects;
using MediatR;

namespace Emp.ApiGateway.Application.Features.Milestones.Queries;

public record GetProjectMilestonesQuery(Guid ProjectId) : IRequest<IReadOnlyList<MilestoneDto>>;
