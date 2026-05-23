using System.Net;
using System.Net.Http.Json;
using Emp.ApiGateway.Application.Interfaces.Infrastructure;
using Emp.ApiGateway.Infrastructure.Configuration;
using EnterpriseMediator.Contracts.Common;
using EnterpriseMediator.Contracts.DTOs.Common;
using EnterpriseMediator.Contracts.DTOs.Financials;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Emp.ApiGateway.Infrastructure.Services;

/// <summary>
/// HTTP Client implementation for communicating with the Financial Microservice.
/// Utilizes HttpClientFactory and configured resilience policies.
/// </summary>
public class FinancialServiceClient : IFinancialServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<FinancialServiceClient> _logger;

    public FinancialServiceClient(
        HttpClient httpClient,
        IOptions<ServiceUrls> serviceUrls,
        ILogger<FinancialServiceClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        var urls = serviceUrls?.Value ?? throw new ArgumentNullException(nameof(serviceUrls));

        if (_httpClient.BaseAddress == null && !string.IsNullOrEmpty(urls.FinancialService))
        {
            _httpClient.BaseAddress = new Uri(urls.FinancialService);
        }
    }

    public async Task<FinancialSummaryResponse?> GetProjectFinancialSummaryAsync(Guid projectId, CancellationToken ct)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/projects/{projectId}/financials", ct);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<FinancialSummaryResponse>(cancellationToken: ct);
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Financial service returned 404 for ProjectId: {ProjectId}", projectId);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Financial service call failed. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, content);
            throw new HttpRequestException($"Financial service call failed with status code {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching financial summary for ProjectId: {ProjectId}", projectId);
            throw;
        }
    }

    public async Task<InvoiceDto> GenerateInvoiceAsync(GenerateInvoiceRequest request, CancellationToken ct)
    {
        _logger.LogInformation("Generating invoice for ProjectId: {ProjectId}, Amount: {Amount} {Currency}",
            request.ProjectId, request.Amount, request.Currency);

        var response = await _httpClient.PostAsJsonAsync("api/v1/invoices/generate", request, ct);

        if (response.IsSuccessStatusCode)
        {
            var invoice = await response.Content.ReadFromJsonAsync<InvoiceDto>(cancellationToken: ct);
            _logger.LogInformation("Invoice generated successfully: {InvoiceId}", invoice!.Id);
            return invoice;
        }

        var content = await response.Content.ReadAsStringAsync(ct);
        _logger.LogError("Failed to generate invoice. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, content);
        throw new HttpRequestException($"Generate invoice failed with status code {response.StatusCode}: {content}");
    }

    public async Task<PagedResultDto<TransactionDto>> GetTransactionsAsync(int page, int pageSize, string? startDate, string? endDate, string? type, string? status, Guid? projectId, CancellationToken ct)
    {
        _logger.LogInformation("Fetching transactions. Page: {Page}, PageSize: {PageSize}", page, pageSize);

        var queryParams = $"?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrEmpty(startDate)) queryParams += $"&startDate={Uri.EscapeDataString(startDate)}";
        if (!string.IsNullOrEmpty(endDate)) queryParams += $"&endDate={Uri.EscapeDataString(endDate)}";
        if (!string.IsNullOrEmpty(type)) queryParams += $"&type={Uri.EscapeDataString(type)}";
        if (!string.IsNullOrEmpty(status)) queryParams += $"&status={Uri.EscapeDataString(status)}";
        if (projectId.HasValue) queryParams += $"&projectId={projectId.Value}";

        var response = await _httpClient.GetAsync($"api/v1/transactions{queryParams}", ct);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<PagedResultDto<TransactionDto>>(cancellationToken: ct)
                   ?? new PagedResultDto<TransactionDto> { Items = [], TotalCount = 0, Page = page, PageSize = pageSize };
        }

        var content = await response.Content.ReadAsStringAsync(ct);
        _logger.LogError("Failed to fetch transactions. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, content);
        throw new HttpRequestException($"Get transactions failed with status code {response.StatusCode}");
    }

    public async Task<Stream> ExportTransactionsCsvAsync(string? startDate, string? endDate, string? type, CancellationToken ct)
    {
        _logger.LogInformation("Exporting transactions CSV");

        var queryParams = "?";
        if (!string.IsNullOrEmpty(startDate)) queryParams += $"startDate={Uri.EscapeDataString(startDate)}&";
        if (!string.IsNullOrEmpty(endDate)) queryParams += $"endDate={Uri.EscapeDataString(endDate)}&";
        if (!string.IsNullOrEmpty(type)) queryParams += $"type={Uri.EscapeDataString(type)}&";
        queryParams = queryParams.TrimEnd('&', '?');

        var response = await _httpClient.GetAsync($"api/v1/transactions/export{queryParams}", ct);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStreamAsync(ct);
        }

        var content = await response.Content.ReadAsStringAsync(ct);
        _logger.LogError("Failed to export transactions CSV. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, content);
        throw new HttpRequestException($"Export transactions CSV failed with status code {response.StatusCode}");
    }

    public async Task InitiatePayoutAsync(Guid projectId, Guid? milestoneId, decimal amount, CancellationToken ct)
    {
        _logger.LogInformation("Initiating payout for ProjectId: {ProjectId}, Amount: {Amount}", projectId, amount);

        var response = await _httpClient.PostAsJsonAsync("api/v1/payouts/initiate",
            new { ProjectId = projectId, MilestoneId = milestoneId, Amount = amount }, ct);

        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Failed to initiate payout. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, content);
            throw new HttpRequestException($"Initiate payout failed with status code {response.StatusCode}: {content}");
        }
    }

    public async Task ApprovePayoutAsync(Guid payoutId, CancellationToken ct)
    {
        _logger.LogInformation("Approving payout: {PayoutId}", payoutId);

        var response = await _httpClient.PostAsync($"api/v1/payouts/{payoutId}/approve", null, ct);

        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Failed to approve payout. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, content);
            throw new HttpRequestException($"Approve payout failed with status code {response.StatusCode}: {content}");
        }
    }

    public async Task RejectPayoutAsync(Guid payoutId, string reason, CancellationToken ct)
    {
        _logger.LogInformation("Rejecting payout: {PayoutId}", payoutId);

        var response = await _httpClient.PostAsJsonAsync($"api/v1/payouts/{payoutId}/reject", new { Reason = reason }, ct);

        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Failed to reject payout. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, content);
            throw new HttpRequestException($"Reject payout failed with status code {response.StatusCode}: {content}");
        }
    }

    public async Task<IReadOnlyList<PayoutDto>> GetPendingPayoutsAsync(CancellationToken ct)
    {
        _logger.LogInformation("Fetching pending payouts");

        var response = await _httpClient.GetAsync("api/v1/payouts?status=Pending", ct);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<IReadOnlyList<PayoutDto>>(cancellationToken: ct) ?? [];
        }

        var content = await response.Content.ReadAsStringAsync(ct);
        _logger.LogError("Failed to fetch pending payouts. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, content);
        throw new HttpRequestException($"Get pending payouts failed with status code {response.StatusCode}");
    }

    public async Task ProcessRefundAsync(Guid projectId, decimal amount, string reason, CancellationToken ct)
    {
        _logger.LogInformation("Processing refund for ProjectId: {ProjectId}, Amount: {Amount}", projectId, amount);

        var response = await _httpClient.PostAsJsonAsync("api/v1/refunds",
            new { ProjectId = projectId, Amount = amount, Reason = reason }, ct);

        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Failed to process refund. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, content);
            throw new HttpRequestException($"Process refund failed with status code {response.StatusCode}: {content}");
        }
    }

    public async Task<InvoiceDto?> GetInvoiceByTokenAsync(string token, CancellationToken ct)
    {
        _logger.LogInformation("Fetching invoice by payment token");

        var response = await _httpClient.GetAsync($"api/v1/invoices/pay/{Uri.EscapeDataString(token)}", ct);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<InvoiceDto>(cancellationToken: ct);
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        var content = await response.Content.ReadAsStringAsync(ct);
        _logger.LogError("Failed to fetch invoice by token. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, content);
        throw new HttpRequestException($"Get invoice by token failed with status code {response.StatusCode}");
    }

    public async Task ConfirmPaymentAsync(Guid invoiceId, string paymentIntentId, CancellationToken ct)
    {
        _logger.LogInformation("Confirming payment for InvoiceId: {InvoiceId}", invoiceId);

        var response = await _httpClient.PostAsJsonAsync($"api/v1/invoices/{invoiceId}/confirm-payment",
            new { PaymentIntentId = paymentIntentId }, ct);

        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Failed to confirm payment. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, content);
            throw new HttpRequestException($"Confirm payment failed with status code {response.StatusCode}: {content}");
        }
    }

    public async Task<DashboardMetricsDto> GetDashboardMetricsAsync(CancellationToken ct)
    {
        _logger.LogInformation("Fetching dashboard metrics");

        var response = await _httpClient.GetAsync("api/v1/dashboard/metrics", ct);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<DashboardMetricsDto>(cancellationToken: ct)
                   ?? throw new InvalidOperationException("Dashboard metrics response was null.");
        }

        var content = await response.Content.ReadAsStringAsync(ct);
        _logger.LogError("Failed to fetch dashboard metrics. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, content);
        throw new HttpRequestException($"Get dashboard metrics failed with status code {response.StatusCode}");
    }

    public async Task<IReadOnlyList<ProfitabilityReportItemDto>> GetProfitabilityReportAsync(CancellationToken ct)
    {
        _logger.LogInformation("Fetching profitability report");

        var response = await _httpClient.GetAsync("api/v1/reports/profitability", ct);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<IReadOnlyList<ProfitabilityReportItemDto>>(cancellationToken: ct) ?? [];
        }

        var content = await response.Content.ReadAsStringAsync(ct);
        _logger.LogError("Failed to fetch profitability report. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, content);
        throw new HttpRequestException($"Get profitability report failed with status code {response.StatusCode}");
    }

    public async Task<RetentionPolicyDto> GetRetentionPolicyAsync(CancellationToken ct)
    {
        _logger.LogInformation("Fetching retention policy");

        var response = await _httpClient.GetAsync("api/v1/config/retention", ct);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<RetentionPolicyDto>(cancellationToken: ct)
                   ?? throw new InvalidOperationException("Retention policy response was null.");
        }

        var content = await response.Content.ReadAsStringAsync(ct);
        _logger.LogError("Failed to fetch retention policy. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, content);
        throw new HttpRequestException($"Get retention policy failed with status code {response.StatusCode}");
    }

    public async Task<RetentionPolicyDto> UpdateRetentionPolicyAsync(RetentionPolicyDto policy, CancellationToken ct)
    {
        _logger.LogInformation("Updating retention policy");

        var response = await _httpClient.PutAsJsonAsync("api/v1/config/retention", policy, ct);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<RetentionPolicyDto>(cancellationToken: ct)
                   ?? throw new InvalidOperationException("Updated retention policy response was null.");
        }

        var content = await response.Content.ReadAsStringAsync(ct);
        _logger.LogError("Failed to update retention policy. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, content);
        throw new HttpRequestException($"Update retention policy failed with status code {response.StatusCode}: {content}");
    }

    public async Task<TaxSettingsDto?> GetTaxSettingsAsync(CancellationToken ct)
    {
        _logger.LogInformation("Fetching tax settings");

        var response = await _httpClient.GetAsync("api/v1/config/tax", ct);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<TaxSettingsDto>(cancellationToken: ct);
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        var content = await response.Content.ReadAsStringAsync(ct);
        _logger.LogError("Failed to fetch tax settings. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, content);
        throw new HttpRequestException($"Get tax settings failed with status code {response.StatusCode}");
    }

    public async Task<TaxSettingsDto> UpdateTaxSettingsAsync(TaxSettingsDto settings, CancellationToken ct)
    {
        _logger.LogInformation("Updating tax settings");

        var response = await _httpClient.PutAsJsonAsync("api/v1/config/tax", settings, ct);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<TaxSettingsDto>(cancellationToken: ct)
                   ?? throw new InvalidOperationException("Updated tax settings response was null.");
        }

        var content = await response.Content.ReadAsStringAsync(ct);
        _logger.LogError("Failed to update tax settings. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, content);
        throw new HttpRequestException($"Update tax settings failed with status code {response.StatusCode}: {content}");
    }
}
