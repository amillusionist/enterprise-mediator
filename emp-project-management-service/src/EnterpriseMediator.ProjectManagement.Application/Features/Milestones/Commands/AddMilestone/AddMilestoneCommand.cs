using EnterpriseMediator.ProjectManagement.Application.Common;
using MediatR;

namespace EnterpriseMediator.ProjectManagement.Application.Features.Milestones.Commands.AddMilestone;

public record AddMilestoneCommand(
    Guid ProjectId,
    string Title,
    string? Description,
    decimal Amount,
    string Currency,
    int Order,
    DateTime? DueDate) : IRequest<Result<Guid>>;
