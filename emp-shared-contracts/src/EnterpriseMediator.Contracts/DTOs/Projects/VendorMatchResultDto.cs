namespace EnterpriseMediator.Contracts.DTOs.Projects;

/// <summary>
/// Result of a semantic vendor matching query using pgvector cosine similarity.
/// </summary>
public record VendorMatchResultDto
{
    public required Guid VendorId { get; init; }
    public required string CompanyName { get; init; }
    public required double SimilarityScore { get; init; }
    public required string[] MatchedSkills { get; init; }
    public string? PrimaryContactEmail { get; init; }
}
