using MediatR;

namespace EnterpriseMediator.ProjectManagement.Application.Features.Milestones.Commands.ApproveMilestone;

public record ApproveMilestoneCommand(
    Guid ProjectId,
    Guid MilestoneId,
    Guid ApprovedByContactId) : IRequest;
