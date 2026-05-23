using EnterpriseMediator.Contracts.Enums;

namespace EnterpriseMediator.Contracts.DTOs.Projects;

/// <summary>
/// Standard API response format for Project entities.
/// Used by API Gateway and Frontend.
/// </summary>
public record ProjectDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required Guid ClientId { get; init; }
    public required ProjectStatus Status { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? StartDate { get; init; }
    public DateTimeOffset? EndDate { get; init; }
    public Guid? AwardedVendorId { get; init; }
}
