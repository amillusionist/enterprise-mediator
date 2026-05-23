using MediatR;

namespace EnterpriseMediator.ProjectManagement.Application.Features.Projects.Commands.ChangeProjectStatus;

public record ChangeProjectStatusCommand(Guid ProjectId, string Action) : IRequest;
