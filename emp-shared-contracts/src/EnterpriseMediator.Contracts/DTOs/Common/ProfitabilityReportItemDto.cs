namespace EnterpriseMediator.Contracts.DTOs.Common;

/// <summary>
/// Per-project profitability breakdown for financial reporting.
/// </summary>
public record ProfitabilityReportItemDto
{
    public required Guid ProjectId { get; init; }
    public required string ProjectName { get; init; }
    public required string ClientName { get; init; }
    public required decimal InvoicedAmount { get; init; }
    public required decimal VendorPayout { get; init; }
    public required decimal PlatformFee { get; init; }
    public required decimal NetProfit { get; init; }
    public required string Currency { get; init; }
}
