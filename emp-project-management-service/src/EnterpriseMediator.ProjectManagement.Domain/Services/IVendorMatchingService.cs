using EnterpriseMediator.ProjectManagement.Domain.Aggregates.ProjectAggregate;

namespace EnterpriseMediator.ProjectManagement.Domain.Services;

public record VendorMatch(Guid VendorId, string CompanyName, double SimilarityScore, List<string> MatchedSkills);

public interface IVendorMatchingService
{
    Task<IReadOnlyList<VendorMatch>> FindMatchingVendorsAsync(
        SowDetails sowDetails,
        int limit = 25,
        double minSimilarityThreshold = 0.75,
        CancellationToken ct = default);
}
