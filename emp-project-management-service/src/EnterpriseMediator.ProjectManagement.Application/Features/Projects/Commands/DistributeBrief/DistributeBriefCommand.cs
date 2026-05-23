using MediatR;

namespace EnterpriseMediator.ProjectManagement.Application.Features.Projects.Commands.DistributeBrief;

public record DistributeBriefCommand(Guid ProjectId) : IRequest;
