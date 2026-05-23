using MediatR;

namespace Emp.ApiGateway.Application.Features.Projects.Commands;

/// <summary>
/// Command to distribute an approved brief to selected vendors for proposal submission.
/// </summary>
public record DistributeBriefCommand(Guid ProjectId, Guid[] VendorIds) : IRequest;
