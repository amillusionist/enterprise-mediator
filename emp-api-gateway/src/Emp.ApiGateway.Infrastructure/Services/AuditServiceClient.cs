using System.Net;
using System.Net.Http.Json;
using Emp.ApiGateway.Application.Interfaces.Infrastructure;
using Emp.ApiGateway.Infrastructure.Configuration;
using EnterpriseMediator.Contracts.Common;
using EnterpriseMediator.Contracts.DTOs.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Emp.ApiGateway.Infrastructure.Services;

/// <summary>
/// HTTP Client implementation for communicating with the downstream Audit Service.
/// Handles retrieval and export of system audit trails.
/// </summary>
public class AuditServiceClient : IAuditServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuditServiceClient> _logger;

    public AuditServiceClient(
        HttpClient httpClient,
        IOptions<ServiceUrls> serviceUrls,
        ILogger<AuditServiceClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        var urls = serviceUrls?.Value ?? throw new ArgumentNullException(nameof(serviceUrls));

        if (_httpClient.BaseAddress == null && !string.IsNullOrEmpty(urls.AuditService))
        {
            _httpClient.BaseAddress = new Uri(urls.AuditService);
        }
    }

    /// <inheritdoc />
    public async Task<PagedResultDto<AuditLogDto>> GetAuditLogsAsync(
        int page, int pageSize, string? userId, string? action,
        string? entityType, string? entityId, string? startDate, string? endDate,
        CancellationToken ct)
    {
        _logger.LogInformation("Fetching audit logs. Page: {Page}, PageSize: {PageSize}", page, pageSize);

        var queryParams = $"?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrEmpty(userId)) queryParams += $"&userId={Uri.EscapeDataString(userId)}";
        if (!string.IsNullOrEmpty(action)) queryParams += $"&action={Uri.EscapeDataString(action)}";
        if (!string.IsNullOrEmpty(entityType)) queryParams += $"&entityType={Uri.EscapeDataString(entityType)}";
        if (!string.IsNullOrEmpty(entityId)) queryParams += $"&entityId={Uri.EscapeDataString(entityId)}";
        if (!string.IsNullOrEmpty(startDate)) queryParams += $"&startDate={Uri.EscapeDataString(startDate)}";
        if (!string.IsNullOrEmpty(endDate)) queryParams += $"&endDate={Uri.EscapeDataString(endDate)}";

        var response = await _httpClient.GetAsync($"api/v1/audit{queryParams}", ct);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<PagedResultDto<AuditLogDto>>(cancellationToken: ct)
                   ?? new PagedResultDto<AuditLogDto> { Items = [], TotalCount = 0, Page = page, PageSize = pageSize };
        }

        var content = await response.Content.ReadAsStringAsync(ct);
        _logger.LogError("Failed to fetch audit logs. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, content);
        throw new HttpRequestException($"Get audit logs failed with status code {response.StatusCode}");
    }

    /// <inheritdoc />
    public async Task<AuditLogDto?> GetAuditLogByIdAsync(Guid id, CancellationToken ct)
    {
        _logger.LogInformation("Fetching audit log entry for ID: {AuditLogId}", id);

        var response = await _httpClient.GetAsync($"api/v1/audit/{id}", ct);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<AuditLogDto>(cancellationToken: ct);
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Audit service returned 404 for AuditLogId: {AuditLogId}", id);
            return null;
        }

        var content = await response.Content.ReadAsStringAsync(ct);
        _logger.LogError("Failed to fetch audit log entry. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, content);
        throw new HttpRequestException($"Get audit log by ID failed with status code {response.StatusCode}");
    }

    /// <inheritdoc />
    public async Task<Stream> ExportAuditLogsCsvAsync(
        string? userId, string? action, string? entityType,
        string? startDate, string? endDate, CancellationToken ct)
    {
        _logger.LogInformation("Exporting audit logs CSV");

        var queryParams = "?";
        if (!string.IsNullOrEmpty(userId)) queryParams += $"userId={Uri.EscapeDataString(userId)}&";
        if (!string.IsNullOrEmpty(action)) queryParams += $"action={Uri.EscapeDataString(action)}&";
        if (!string.IsNullOrEmpty(entityType)) queryParams += $"entityType={Uri.EscapeDataString(entityType)}&";
        if (!string.IsNullOrEmpty(startDate)) queryParams += $"startDate={Uri.EscapeDataString(startDate)}&";
        if (!string.IsNullOrEmpty(endDate)) queryParams += $"endDate={Uri.EscapeDataString(endDate)}&";
        queryParams = queryParams.TrimEnd('&', '?');

        var response = await _httpClient.GetAsync($"api/v1/audit/export{queryParams}", ct);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStreamAsync(ct);
        }

        var content = await response.Content.ReadAsStringAsync(ct);
        _logger.LogError("Failed to export audit logs CSV. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, content);
        throw new HttpRequestException($"Export audit logs CSV failed with status code {response.StatusCode}");
    }
}
