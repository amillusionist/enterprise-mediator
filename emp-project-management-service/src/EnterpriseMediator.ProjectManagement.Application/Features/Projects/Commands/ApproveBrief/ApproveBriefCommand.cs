using MediatR;

namespace EnterpriseMediator.ProjectManagement.Application.Features.Projects.Commands.ApproveBrief;

public record ApproveBriefCommand(Guid ProjectId) : IRequest;
