using EnterpriseMediator.ProjectManagement.Application.Common;
using MediatR;

namespace EnterpriseMediator.ProjectManagement.Application.Features.Projects.Commands.CreateProject;

public record CreateProjectCommand(
    Guid ClientId,
    string Name,
    string Description) : IRequest<Result<Guid>>;
