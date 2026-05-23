namespace EnterpriseMediator.Contracts.DTOs.Common;

/// <summary>
/// Full audit log entry with change details and metadata.
/// </summary>
public record AuditLogDto
{
    public required Guid Id { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
    public required string UserId { get; init; }
    public string? UserName { get; init; }
    public string? UserEmail { get; init; }
    public required string ActionType { get; init; }
    public required string EntityType { get; init; }
    public required string EntityId { get; init; }
    public string? EntityName { get; init; }
    public Dictionary<string, object>? Changes { get; init; }
    public Dictionary<string, object>? Metadata { get; init; }
    public required string Status { get; init; }
}
