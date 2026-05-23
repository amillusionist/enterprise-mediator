using MediatR;

namespace Emp.ApiGateway.Application.Features.Projects.Commands;

/// <summary>
/// Command to update the status of a project.
/// </summary>
public record UpdateProjectStatusCommand(Guid ProjectId, string Status) : IRequest;
