using EnterpriseMediator.Contracts.DTOs.Projects;
using MediatR;

namespace Emp.ApiGateway.Application.Features.Briefs.Commands;

public record ApproveProjectBriefCommand(Guid ProjectId, UpdateProjectBriefRequest? Edits) : IRequest<ProjectBriefDto>;
