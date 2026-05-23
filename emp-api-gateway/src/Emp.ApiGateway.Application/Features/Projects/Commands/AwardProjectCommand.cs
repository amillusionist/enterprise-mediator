using MediatR;

namespace Emp.ApiGateway.Application.Features.Projects.Commands;

/// <summary>
/// Command to award a project to a specific vendor.
/// </summary>
public record AwardProjectCommand(Guid ProjectId, Guid VendorId) : IRequest;
