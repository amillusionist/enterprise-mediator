using System.Net.Mime;
using Emp.ApiGateway.Application.Features.Financials.Commands;
using Emp.ApiGateway.Application.Features.Projects.Queries;
using Emp.ApiGateway.Application.Interfaces.Infrastructure;
using EnterpriseMediator.Contracts.Common;
using EnterpriseMediator.Contracts.DTOs.Common;
using EnterpriseMediator.Contracts.DTOs.Financials;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Emp.ApiGateway.Web.Controllers;

/// <summary>
/// Public API Controller for Financial operations.
/// Handles aggregated financial views, invoice generation, transactions, payouts,
/// refunds, reporting, and configuration via the Financial Microservice.
/// </summary>
[ApiController]
[Route("api/v1/financials")]
[Authorize]
[Produces(MediaTypeNames.Application.Json)]
public class FinancialsController : ControllerBase
{
    private readonly ISender _mediator;
    private readonly IFinancialServiceClient _financialService;
    private readonly ILogger<FinancialsController> _logger;

    public FinancialsController(
        ISender mediator,
        IFinancialServiceClient financialService,
        ILogger<FinancialsController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _financialService = financialService ?? throw new ArgumentNullException(nameof(financialService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // ──────────────────────────────────────────
    // Project Financials
    // ──────────────────────────────────────────

    /// <summary>
    /// Retrieves a summary of financials for a specific project.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Financial summary retrieved.</response>
    /// <response code="404">Project financials not found.</response>
    [HttpGet("projects/{projectId:guid}")]
    [ProducesResponseType(typeof(PublicFinancialSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PublicFinancialSummaryDto>> GetProjectFinancialSummary(
        [FromRoute] Guid projectId,
        CancellationToken ct)
    {
        _logger.LogDebug("Retrieving financial summary for project: {ProjectId}", projectId);

        var query = new GetProjectDashboardQuery(projectId);
        var result = await _mediator.Send(query, ct);

        if (result?.Financials == null)
        {
            _logger.LogWarning("Financial data not found for project: {ProjectId}", projectId);
            return NotFound($"Financial records for project {projectId} not found.");
        }

        return Ok(result.Financials);
    }

    /// <summary>
    /// Generates a client invoice for a project.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="command">Invoice generation details.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="201">Invoice generated successfully.</response>
    /// <response code="400">Invalid invoice data.</response>
    [HttpPost("projects/{projectId:guid}/invoices/generate")]
    [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<InvoiceDto>> GenerateInvoice(
        [FromRoute] Guid projectId,
        [FromBody] GenerateInvoiceCommand command,
        CancellationToken ct)
    {
        _logger.LogInformation("Invoice generation requested for project {ProjectId}", projectId);

        if (command.ProjectId != projectId)
        {
            return BadRequest("Project ID in route does not match the request body.");
        }

        var invoice = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetProjectFinancialSummary), new { projectId }, invoice);
    }

    // ──────────────────────────────────────────
    // Transactions
    // ──────────────────────────────────────────

    /// <summary>
    /// Retrieves a paginated list of financial transactions with optional filters.
    /// </summary>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="startDate">Optional start date filter (ISO 8601).</param>
    /// <param name="endDate">Optional end date filter (ISO 8601).</param>
    /// <param name="type">Optional transaction type filter.</param>
    /// <param name="status">Optional transaction status filter.</param>
    /// <param name="projectId">Optional project ID filter.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Paginated list of transactions.</returns>
    /// <response code="200">Transactions retrieved successfully.</response>
    [HttpGet("/api/v1/finance/transactions")]
    [ProducesResponseType(typeof(PagedResultDto<TransactionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResultDto<TransactionDto>>> GetTransactions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? startDate = null,
        [FromQuery] string? endDate = null,
        [FromQuery] string? type = null,
        [FromQuery] string? status = null,
        [FromQuery] Guid? projectId = null,
        CancellationToken ct = default)
    {
        var result = await _financialService.GetTransactionsAsync(
            page, pageSize, startDate, endDate, type, status, projectId, ct);
        return Ok(result);
    }

    /// <summary>
    /// Exports transactions as a CSV file for the given filters.
    /// </summary>
    /// <param name="startDate">Optional start date filter (ISO 8601).</param>
    /// <param name="endDate">Optional end date filter (ISO 8601).</param>
    /// <param name="type">Optional transaction type filter.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>CSV file stream.</returns>
    /// <response code="200">CSV file generated.</response>
    [HttpGet("/api/v1/finance/reports/transactions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportTransactionsCsv(
        [FromQuery] string? startDate = null,
        [FromQuery] string? endDate = null,
        [FromQuery] string? type = null,
        CancellationToken ct = default)
    {
        var csvStream = await _financialService.ExportTransactionsCsvAsync(startDate, endDate, type, ct);
        return File(csvStream, "text/csv", "transactions.csv");
    }

    // ──────────────────────────────────────────
    // Payouts
    // ──────────────────────────────────────────

    /// <summary>
    /// Retrieves all payouts awaiting approval.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of pending payouts.</returns>
    /// <response code="200">Pending payouts retrieved.</response>
    [HttpGet("/api/v1/finance/payouts")]
    [ProducesResponseType(typeof(IReadOnlyList<PayoutDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PayoutDto>>> GetPendingPayouts(CancellationToken ct)
    {
        var result = await _financialService.GetPendingPayoutsAsync(ct);
        return Ok(result);
    }

    /// <summary>
    /// Initiates a vendor payout for a project or milestone.
    /// </summary>
    /// <param name="request">Payout initiation details.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Payout initiated.</response>
    /// <response code="400">Invalid payout data.</response>
    [HttpPost("/api/v1/finance/payouts/initiate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> InitiatePayout(
        [FromBody] InitiatePayoutRequest request,
        CancellationToken ct)
    {
        _logger.LogInformation("Initiating payout for project {ProjectId}, amount: {Amount}", request.ProjectId, request.Amount);
        await _financialService.InitiatePayoutAsync(request.ProjectId, request.MilestoneId, request.Amount, ct);
        return Ok(new { message = "Payout initiated." });
    }

    /// <summary>
    /// Approves a pending vendor payout.
    /// </summary>
    /// <param name="payoutId">The payout identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Payout approved.</response>
    [HttpPost("/api/v1/finance/payouts/{payoutId:guid}/approve")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ApprovePayout(
        [FromRoute] Guid payoutId,
        CancellationToken ct)
    {
        _logger.LogInformation("Approving payout: {PayoutId}", payoutId);
        await _financialService.ApprovePayoutAsync(payoutId, ct);
        return Ok(new { message = "Payout approved." });
    }

    /// <summary>
    /// Rejects a pending vendor payout with a reason.
    /// </summary>
    /// <param name="payoutId">The payout identifier.</param>
    /// <param name="request">Rejection reason.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Payout rejected.</response>
    [HttpPost("/api/v1/finance/payouts/{payoutId:guid}/reject")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RejectPayout(
        [FromRoute] Guid payoutId,
        [FromBody] RejectPayoutRequest request,
        CancellationToken ct)
    {
        _logger.LogInformation("Rejecting payout: {PayoutId}, Reason: {Reason}", payoutId, request.Reason);
        await _financialService.RejectPayoutAsync(payoutId, request.Reason, ct);
        return Ok(new { message = "Payout rejected." });
    }

    // ──────────────────────────────────────────
    // Refunds
    // ──────────────────────────────────────────

    /// <summary>
    /// Processes a refund for a project.
    /// </summary>
    /// <param name="request">Refund details.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Refund processed.</response>
    /// <response code="400">Invalid refund data.</response>
    [HttpPost("/api/v1/finance/refunds")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ProcessRefund(
        [FromBody] ProcessRefundRequest request,
        CancellationToken ct)
    {
        _logger.LogInformation("Processing refund for project {ProjectId}, amount: {Amount}", request.ProjectId, request.Amount);
        await _financialService.ProcessRefundAsync(request.ProjectId, request.Amount, request.Reason, ct);
        return Ok(new { message = "Refund processed." });
    }

    // ──────────────────────────────────────────
    // Configuration
    // ──────────────────────────────────────────

    /// <summary>
    /// Retrieves the current tax settings configuration.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Tax settings.</returns>
    /// <response code="200">Tax settings retrieved.</response>
    [HttpGet("/api/v1/finance/config/tax")]
    [ProducesResponseType(typeof(TaxSettingsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<TaxSettingsDto>> GetTaxSettings(CancellationToken ct)
    {
        var result = await _financialService.GetTaxSettingsAsync(ct);
        return Ok(result);
    }

    /// <summary>
    /// Updates the tax settings configuration.
    /// </summary>
    /// <param name="settings">Updated tax settings.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Updated tax settings.</returns>
    /// <response code="200">Tax settings updated.</response>
    [HttpPut("/api/v1/finance/config/tax")]
    [ProducesResponseType(typeof(TaxSettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TaxSettingsDto>> UpdateTaxSettings(
        [FromBody] TaxSettingsDto settings,
        CancellationToken ct)
    {
        _logger.LogInformation("Updating tax settings");
        var result = await _financialService.UpdateTaxSettingsAsync(settings, ct);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves the current data retention policy configuration.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Retention policy.</returns>
    /// <response code="200">Retention policy retrieved.</response>
    [HttpGet("/api/v1/finance/config/retention")]
    [ProducesResponseType(typeof(RetentionPolicyDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<RetentionPolicyDto>> GetRetentionPolicy(CancellationToken ct)
    {
        var result = await _financialService.GetRetentionPolicyAsync(ct);
        return Ok(result);
    }

    /// <summary>
    /// Updates the data retention policy configuration.
    /// </summary>
    /// <param name="policy">Updated retention policy.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Updated retention policy.</returns>
    /// <response code="200">Retention policy updated.</response>
    [HttpPut("/api/v1/finance/config/retention")]
    [ProducesResponseType(typeof(RetentionPolicyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RetentionPolicyDto>> UpdateRetentionPolicy(
        [FromBody] RetentionPolicyDto policy,
        CancellationToken ct)
    {
        _logger.LogInformation("Updating retention policy");
        var result = await _financialService.UpdateRetentionPolicyAsync(policy, ct);
        return Ok(result);
    }

    // ──────────────────────────────────────────
    // Reports
    // ──────────────────────────────────────────

    /// <summary>
    /// Retrieves per-project profitability report data.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Profitability report items.</returns>
    /// <response code="200">Report generated successfully.</response>
    [HttpGet("/api/v1/finance/reports/profitability")]
    [ProducesResponseType(typeof(IReadOnlyList<ProfitabilityReportItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProfitabilityReportItemDto>>> GetProfitabilityReport(CancellationToken ct)
    {
        var result = await _financialService.GetProfitabilityReportAsync(ct);
        return Ok(result);
    }

    // ──────────────────────────────────────────
    // Dashboard Metrics
    // ──────────────────────────────────────────

    /// <summary>
    /// Retrieves aggregated dashboard metrics for the admin overview screen.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Dashboard metrics.</returns>
    /// <response code="200">Metrics retrieved successfully.</response>
    [HttpGet("/api/v1/dashboard/metrics")]
    [ProducesResponseType(typeof(DashboardMetricsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardMetricsDto>> GetDashboardMetrics(CancellationToken ct)
    {
        var result = await _financialService.GetDashboardMetricsAsync(ct);
        return Ok(result);
    }

    // ──────────────────────────────────────────
    // Public Invoice Endpoints
    // ──────────────────────────────────────────

    /// <summary>
    /// Public endpoint to retrieve invoice details for payment via a secure token link.
    /// </summary>
    /// <param name="token">The secure invoice payment token.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Invoice details for payment.</returns>
    /// <response code="200">Invoice retrieved.</response>
    /// <response code="404">Invoice not found or token expired.</response>
    [HttpGet("/api/v1/public/invoices/pay/{token}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InvoiceDto>> GetInvoiceByToken(
        [FromRoute] string token,
        CancellationToken ct)
    {
        var result = await _financialService.GetInvoiceByTokenAsync(token, ct);

        if (result == null)
        {
            return NotFound("Invoice not found or payment link expired.");
        }

        return Ok(result);
    }

    /// <summary>
    /// Public endpoint to confirm a Stripe payment for an invoice.
    /// </summary>
    /// <param name="invoiceId">The invoice identifier.</param>
    /// <param name="request">Stripe payment confirmation details.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Payment confirmed.</response>
    /// <response code="400">Invalid payment data.</response>
    [HttpPost("/api/v1/public/invoices/{invoiceId:guid}/confirm-payment")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmPayment(
        [FromRoute] Guid invoiceId,
        [FromBody] ConfirmPaymentRequest request,
        CancellationToken ct)
    {
        _logger.LogInformation("Payment confirmation for invoice {InvoiceId}", invoiceId);
        await _financialService.ConfirmPaymentAsync(invoiceId, request.PaymentIntentId, ct);
        return Ok(new { message = "Payment confirmed." });
    }
}

/// <summary>
/// Request payload for initiating a vendor payout.
/// </summary>
public record InitiatePayoutRequest(Guid ProjectId, Guid? MilestoneId, decimal Amount);

/// <summary>
/// Request payload for rejecting a payout.
/// </summary>
public record RejectPayoutRequest(string Reason);

/// <summary>
/// Request payload for processing a refund.
/// </summary>
public record ProcessRefundRequest(Guid ProjectId, decimal Amount, string Reason);

/// <summary>
/// Request payload for confirming a Stripe payment.
/// </summary>
public record ConfirmPaymentRequest(string PaymentIntentId);
