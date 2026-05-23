using EnterpriseMediator.ProjectManagement.Domain.Services;
using MediatR;

namespace EnterpriseMediator.ProjectManagement.Application.Features.Projects.Queries.GetMatchingVendors;

public record GetMatchingVendorsQuery(
    Guid ProjectId,
    int Limit = 25,
    double MinSimilarity = 0.75) : IRequest<IReadOnlyList<VendorMatch>>;
