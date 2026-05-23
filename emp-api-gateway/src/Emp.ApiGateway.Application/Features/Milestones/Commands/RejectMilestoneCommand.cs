using EnterpriseMediator.Contracts.DTOs.Projects;
using MediatR;

namespace Emp.ApiGateway.Application.Features.Milestones.Commands;

public record RejectMilestoneCommand(Guid MilestoneId, string Reason) : IRequest<MilestoneDto>;
