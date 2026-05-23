using EnterpriseMediator.Contracts.DTOs.Projects;

namespace EnterpriseMediator.Contracts.DTOs.Common;

/// <summary>
/// Aggregated dashboard metrics for the admin overview screen.
/// </summary>
public record DashboardMetricsDto
{
    public int ActiveProjectsCount { get; init; }
    public int PendingSowCount { get; init; }
    public int ProposalsAwaitingCount { get; init; }
    public decimal TotalRevenue { get; init; }
    public decimal TotalPayouts { get; init; }
    public decimal NetProfit { get; init; }
    public required Dictionary<string, int> ProjectsByStatus { get; init; }
    public required IReadOnlyList<MilestoneDto> UpcomingMilestones { get; init; }
    public required IReadOnlyList<AuditLogSummaryDto> RecentActivity { get; init; }
}

/// <summary>
/// Compact audit log entry for dashboard recent activity feeds.
/// </summary>
public record AuditLogSummaryDto
{
    public required Guid Id { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
    public required string UserId { get; init; }
    public string? UserName { get; init; }
    public required string ActionType { get; init; }
    public required string EntityType { get; init; }
    public required string EntityId { get; init; }
    public string? EntityName { get; init; }
    public required string Status { get; init; }
}
