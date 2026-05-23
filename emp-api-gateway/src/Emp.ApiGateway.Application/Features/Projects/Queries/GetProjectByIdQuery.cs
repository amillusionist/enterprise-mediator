using EnterpriseMediator.Contracts.DTOs.Projects;
using MediatR;

namespace Emp.ApiGateway.Application.Features.Projects.Queries;

/// <summary>
/// Query to retrieve a specific project by its unique identifier.
/// </summary>
public record GetProjectByIdQuery(Guid ProjectId) : IRequest<ProjectDto?>;
