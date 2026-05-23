using EnterpriseMediator.Contracts.DTOs.Projects;
using MediatR;

namespace Emp.ApiGateway.Application.Features.Projects.Queries;

/// <summary>
/// Query to retrieve the SOW processing status for a project.
/// </summary>
public record GetSowStatusQuery(Guid ProjectId) : IRequest<SowDocumentDto?>;
