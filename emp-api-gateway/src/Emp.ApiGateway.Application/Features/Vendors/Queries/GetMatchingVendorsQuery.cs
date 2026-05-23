using EnterpriseMediator.Contracts.DTOs.Projects;
using MediatR;

namespace Emp.ApiGateway.Application.Features.Vendors.Queries;

public record GetMatchingVendorsQuery(Guid ProjectId, int MaxResults = 25, double MinScore = 0.75) : IRequest<IReadOnlyList<VendorMatchResultDto>>;
