using EnterpriseMediator.ProjectManagement.Application.Common;
using MediatR;

namespace EnterpriseMediator.ProjectManagement.Application.Features.Projects.Commands.AwardProject;

public record AwardProjectCommand(Guid ProjectId, Guid ProposalId) : IRequest<Result>;
