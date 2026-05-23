namespace EnterpriseMediator.Financial.Application.DTOs;

public record FinancialSummaryDto
{
    public Guid ProjectId { get; init; }
    public decimal TotalBudget { get; init; }
    public decimal TotalInvoiced { get; init; }
    public decimal TotalPaid { get; init; }
    public decimal PendingPayouts { get; init; }
    public string Currency { get; init; } = "USD";
    public bool HasOverdueInvoices { get; init; }
}
