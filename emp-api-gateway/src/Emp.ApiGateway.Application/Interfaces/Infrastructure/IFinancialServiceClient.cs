using EnterpriseMediator.Contracts.Common;
using EnterpriseMediator.Contracts.DTOs.Common;
using EnterpriseMediator.Contracts.DTOs.Financials;

namespace Emp.ApiGateway.Application.Interfaces.Infrastructure;

/// <summary>
/// Contract for communicating with the downstream Financial Microservice.
/// Handles aggregation of financial data, invoice, payout, and reporting operations.
/// </summary>
public interface IFinancialServiceClient
{
    /// <summary>
    /// Retrieves the financial summary for a specific project.
    /// </summary>
    Task<FinancialSummaryResponse?> GetProjectFinancialSummaryAsync(Guid projectId, CancellationToken ct);

    /// <summary>
    /// Sends a request to generate a client invoice for a project.
    /// </summary>
    Task<InvoiceDto> GenerateInvoiceAsync(GenerateInvoiceRequest request, CancellationToken ct);

    /// <summary>
    /// Retrieves a paginated list of financial transactions with optional filters.
    /// </summary>
    Task<PagedResultDto<TransactionDto>> GetTransactionsAsync(int page, int pageSize, string? startDate, string? endDate, string? type, string? status, Guid? projectId, CancellationToken ct);

    /// <summary>
    /// Exports transactions as a CSV stream for the given filters.
    /// </summary>
    Task<Stream> ExportTransactionsCsvAsync(string? startDate, string? endDate, string? type, CancellationToken ct);

    /// <summary>
    /// Initiates a vendor payout for a project or milestone.
    /// </summary>
    Task InitiatePayoutAsync(Guid projectId, Guid? milestoneId, decimal amount, CancellationToken ct);

    /// <summary>
    /// Approves a pending vendor payout.
    /// </summary>
    Task ApprovePayoutAsync(Guid payoutId, CancellationToken ct);

    /// <summary>
    /// Rejects a pending vendor payout with a reason.
    /// </summary>
    Task RejectPayoutAsync(Guid payoutId, string reason, CancellationToken ct);

    /// <summary>
    /// Retrieves all payouts awaiting approval.
    /// </summary>
    Task<IReadOnlyList<PayoutDto>> GetPendingPayoutsAsync(CancellationToken ct);

    /// <summary>
    /// Processes a refund for a project.
    /// </summary>
    Task ProcessRefundAsync(Guid projectId, decimal amount, string reason, CancellationToken ct);

    /// <summary>
    /// Retrieves an invoice by its public payment token.
    /// </summary>
    Task<InvoiceDto?> GetInvoiceByTokenAsync(string token, CancellationToken ct);

    /// <summary>
    /// Confirms a Stripe payment for an invoice.
    /// </summary>
    Task ConfirmPaymentAsync(Guid invoiceId, string paymentIntentId, CancellationToken ct);

    /// <summary>
    /// Retrieves aggregated dashboard metrics for the admin overview.
    /// </summary>
    Task<DashboardMetricsDto> GetDashboardMetricsAsync(CancellationToken ct);

    /// <summary>
    /// Retrieves per-project profitability report data.
    /// </summary>
    Task<IReadOnlyList<ProfitabilityReportItemDto>> GetProfitabilityReportAsync(CancellationToken ct);

    /// <summary>
    /// Retrieves the current data retention policy configuration.
    /// </summary>
    Task<RetentionPolicyDto> GetRetentionPolicyAsync(CancellationToken ct);

    /// <summary>
    /// Updates the data retention policy configuration.
    /// </summary>
    Task<RetentionPolicyDto> UpdateRetentionPolicyAsync(RetentionPolicyDto policy, CancellationToken ct);

    /// <summary>
    /// Retrieves the current tax settings configuration.
    /// </summary>
    Task<TaxSettingsDto?> GetTaxSettingsAsync(CancellationToken ct);

    /// <summary>
    /// Updates the tax settings configuration.
    /// </summary>
    Task<TaxSettingsDto> UpdateTaxSettingsAsync(TaxSettingsDto settings, CancellationToken ct);
}

/// <summary>
/// Data Transfer Object representing the financial state of a project.
/// Kept as a gateway-local DTO to avoid tight coupling with the shared contract
/// until the financial service also references the shared contracts.
/// </summary>
public record FinancialSummaryResponse
{
    public Guid ProjectId { get; init; }
    public decimal TotalBudget { get; init; }
    public decimal TotalInvoiced { get; init; }
    public decimal TotalPaid { get; init; }
    public decimal PendingPayouts { get; init; }
    public string Currency { get; init; } = "USD";
    public bool HasOverdueInvoices { get; init; }
}
