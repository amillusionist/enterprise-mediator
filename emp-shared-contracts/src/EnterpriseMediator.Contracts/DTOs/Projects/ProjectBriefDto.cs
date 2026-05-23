namespace EnterpriseMediator.Contracts.DTOs.Projects;

/// <summary>
/// AI-extracted structured data from a Statement of Work.
/// Contains skills, scope, timeline, and other extracted fields.
/// </summary>
public record ProjectBriefDto
{
    public required Guid Id { get; init; }
    public required Guid ProjectId { get; init; }
    public required string Title { get; init; }
    public string? Summary { get; init; }
    public required string[] RequiredSkills { get; init; }
    public string[]? Technologies { get; init; }
    public string? Scope { get; init; }
    public string[]? Deliverables { get; init; }
    public int? EstimatedDurationWeeks { get; init; }
    public decimal? EstimatedBudget { get; init; }
    public string? Currency { get; init; }
    public string? ComplexityLevel { get; init; }
    public DateTimeOffset? ExtractedAt { get; init; }
    public bool IsApproved { get; init; }
}
