using EnterpriseMediator.ProjectManagement.Domain.Aggregates.ProjectAggregate;
using EnterpriseMediator.ProjectManagement.Domain.Services;
using EnterpriseMediator.ProjectManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EnterpriseMediator.ProjectManagement.Infrastructure.Services;

public class VectorVendorMatchingService : IVendorMatchingService
{
    private readonly ProjectDbContext _dbContext;
    private readonly ILogger<VectorVendorMatchingService> _logger;

    public VectorVendorMatchingService(ProjectDbContext dbContext, ILogger<VectorVendorMatchingService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<IReadOnlyList<VendorMatch>> FindMatchingVendorsAsync(
        SowDetails sowDetails, int limit = 25, double minSimilarityThreshold = 0.75, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(sowDetails);

        if (!sowDetails.IsPopulated())
        {
            _logger.LogWarning("SOW details are not populated. Cannot perform vendor matching");
            return Array.Empty<VendorMatch>();
        }

        if (limit <= 0) limit = 25;

        _logger.LogInformation("Executing vendor matching with {SkillCount} skills, limit {Limit}", sowDetails.RequiredSkills.Count, limit);

        try
        {
            var skillsList = string.Join(", ", sowDetails.RequiredSkills);

            var matches = await _dbContext.Database
                .SqlQuery<VendorMatchResult>(
                    $@"SELECT v.""Id"" AS ""VendorId"", v.""CompanyName"",
                       COALESCE(1 - MIN(s.""VectorEmbedding"" <=> query.embedding), 0) AS ""SimilarityScore""
                    FROM ""Vendors"" v
                    JOIN ""VendorSkills"" vs ON vs.""VendorId"" = v.""Id""
                    JOIN ""Skills"" s ON s.""Id"" = vs.""SkillId""
                    CROSS JOIN LATERAL (
                        SELECT AVG(s2.""VectorEmbedding"") as embedding
                        FROM ""Skills"" s2
                        WHERE s2.""Name"" = ANY(string_to_array({skillsList}, ', '))
                    ) query
                    WHERE v.""IsActive"" = true
                    GROUP BY v.""Id"", v.""CompanyName""
                    HAVING COALESCE(1 - MIN(s.""VectorEmbedding"" <=> query.embedding), 0) >= {minSimilarityThreshold}
                    ORDER BY ""SimilarityScore"" DESC
                    LIMIT {limit}")
                .ToListAsync(ct);

            return matches.Select(m => new VendorMatch(
                m.VendorId, m.CompanyName, m.SimilarityScore,
                sowDetails.RequiredSkills.ToList())).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing vendor matching search");
            throw new InvalidOperationException("Failed to execute vendor matching search.", ex);
        }
    }

    private sealed class VendorMatchResult
    {
        public Guid VendorId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public double SimilarityScore { get; set; }
    }
}
