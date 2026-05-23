using EnterpriseMediator.Contracts.Enums;

namespace EnterpriseMediator.Contracts.DTOs.Projects;

/// <summary>
/// Project milestone details for the approval workflow.
/// </summary>
public record MilestoneDto
{
    public required Guid Id { get; init; }
    public required Guid ProjectId { get; init; }
    public required string Title { get; init; }
    public string? Description { get; init; }
    public required decimal Amount { get; init; }
    public required string Currency { get; init; }
    public required MilestoneStatus Status { get; init; }
    public DateTimeOffset? DueDate { get; init; }
    public DateTimeOffset? ApprovedAt { get; init; }
    public string? RejectionReason { get; init; }
}
