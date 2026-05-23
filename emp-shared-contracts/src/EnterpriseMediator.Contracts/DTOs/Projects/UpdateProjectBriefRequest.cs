namespace EnterpriseMediator.Contracts.DTOs.Projects;

/// <summary>
/// Request payload for admin edits to an AI-extracted brief before approval.
/// </summary>
public record UpdateProjectBriefRequest
{
    public string? Title { get; init; }
    public string? Summary { get; init; }
    public string[]? RequiredSkills { get; init; }
    public string[]? Technologies { get; init; }
    public string? Scope { get; init; }
    public string[]? Deliverables { get; init; }
    public int? EstimatedDurationWeeks { get; init; }
    public decimal? EstimatedBudget { get; init; }
    public string? Currency { get; init; }
}
