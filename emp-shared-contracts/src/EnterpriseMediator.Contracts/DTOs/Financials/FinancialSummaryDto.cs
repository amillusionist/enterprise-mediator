namespace EnterpriseMediator.Contracts.DTOs.Financials;

/// <summary>
/// Aggregated financial summary for a project. Used by the API Gateway BFF pattern.
/// </summary>
public record FinancialSummaryDto
{
    public required Guid ProjectId { get; init; }
    public required decimal TotalBudget { get; init; }
    public required decimal TotalInvoiced { get; init; }
    public required decimal TotalPaid { get; init; }
    public required decimal PendingPayouts { get; init; }
    public required string Currency { get; init; }
    public required bool HasOverdueInvoices { get; init; }
}
