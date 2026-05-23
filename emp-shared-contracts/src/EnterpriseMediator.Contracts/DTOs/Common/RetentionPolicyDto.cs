namespace EnterpriseMediator.Contracts.DTOs.Common;

/// <summary>
/// Data retention policy configuration for system administration.
/// </summary>
public record RetentionPolicyDto
{
    public int AuditLogRetentionDays { get; init; }
    public int FinancialRecordRetentionDays { get; init; }
    public int ProjectDataRetentionDays { get; init; }
}
