using EnterpriseMediator.Contracts.DTOs.Projects;
using MediatR;

namespace Emp.ApiGateway.Application.Features.Projects.Commands;

/// <summary>
/// Command to update the project brief with admin edits.
/// </summary>
public record UpdateBriefCommand(Guid ProjectId, UpdateProjectBriefRequest Request) : IRequest;
