using EnterpriseMediator.Contracts.DTOs.Projects;
using MediatR;

namespace Emp.ApiGateway.Application.Features.Milestones.Commands;

public record ApproveMilestoneCommand(Guid MilestoneId) : IRequest<MilestoneDto>;
