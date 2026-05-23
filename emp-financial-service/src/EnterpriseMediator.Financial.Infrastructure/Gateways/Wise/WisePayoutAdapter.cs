using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using EnterpriseMediator.Financial.Domain.Entities;
using EnterpriseMediator.Financial.Domain.Interfaces;
using EnterpriseMediator.Financial.Infrastructure.Persistence.Configurations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EnterpriseMediator.Financial.Infrastructure.Gateways.Wise
{
    public class WisePayoutAdapter : IPayoutGateway
    {
        private readonly HttpClient _httpClient;
        private readonly WiseSettings _settings;
        private readonly ILogger<WisePayoutAdapter> _logger;

        private const string QuotesEndpoint = "v2/quotes";
        private const string TransfersEndpoint = "v1/transfers";

        public WisePayoutAdapter(
            HttpClient httpClient,
            IOptions<WiseSettings> settings,
            ILogger<WisePayoutAdapter> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            ValidateConfiguration();
            ConfigureHttpClient();
        }

        private void ValidateConfiguration()
        {
            if (string.IsNullOrWhiteSpace(_settings.ApiToken))
                throw new InvalidOperationException("Wise API Token is not configured.");

            if (_settings.ProfileId <= 0)
                throw new InvalidOperationException("Wise Profile ID is invalid.");

            if (string.IsNullOrWhiteSpace(_settings.BaseUrl))
                throw new InvalidOperationException("Wise Base URL is not configured.");
        }

        private void ConfigureHttpClient()
        {
            _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiToken);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<PayoutResult> ExecutePayoutAsync(Payout payout, string idempotencyKey, CancellationToken cancellationToken)
        {
            if (payout == null) throw new ArgumentNullException(nameof(payout));
            if (string.IsNullOrWhiteSpace(idempotencyKey)) throw new ArgumentNullException(nameof(idempotencyKey));

            _logger.LogInformation("Initiating Wise payout for PayoutId {PayoutId}", payout.Id);

            try
            {
                var quoteId = await CreateQuoteAsync(payout, cancellationToken);
                _logger.LogDebug("Wise Quote created: {QuoteId} for Payout {PayoutId}", quoteId, payout.Id);

                var (transferId, status, fee, feeCurrency, estimatedDelivery) =
                    await CreateTransferAsync(payout, quoteId, idempotencyKey, cancellationToken);
                _logger.LogInformation("Wise Transfer created: {TransferId} for Payout {PayoutId}", transferId, payout.Id);

                return new PayoutResult
                {
                    TransferId = transferId,
                    Status = status,
                    FeeAmount = fee,
                    FeeCurrency = feeCurrency,
                    EstimatedDelivery = estimatedDelivery
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP Error executing Wise payout for PayoutId {PayoutId}. Status: {StatusCode}", payout.Id, ex.StatusCode);
                throw new InvalidOperationException($"Wise Payout failed due to network or API error: {ex.Message}", ex);
            }
        }

        public async Task<string> GetPayoutStatusAsync(string transferId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(transferId))
                throw new ArgumentException("Transfer ID is required.", nameof(transferId));

            var response = await _httpClient.GetAsync($"{TransfersEndpoint}/{transferId}", cancellationToken);
            await EnsureSuccessAsync(response, "Get Transfer Status");

            var result = await response.Content.ReadFromJsonAsync<WiseTransferDetailResponse>(cancellationToken: cancellationToken);
            return result?.Status ?? "unknown";
        }

        public async Task<bool> ValidateRecipientAsync(Guid vendorId, string currency, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Validating Wise recipient for Vendor {VendorId} Currency {Currency}", vendorId, currency);

            try
            {
                var response = await _httpClient.GetAsync(
                    $"v1/accounts?profile={_settings.ProfileId}&currency={currency}",
                    cancellationToken);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to validate recipient for Vendor {VendorId}", vendorId);
                return false;
            }
        }

        private async Task<string> CreateQuoteAsync(Payout payout, CancellationToken ct)
        {
            var request = new
            {
                profile = _settings.ProfileId,
                sourceCurrency = payout.Amount.Currency.Code,
                targetCurrency = payout.Amount.Currency.Code,
                targetAmount = payout.Amount.Amount,
                payOut = "BALANCE"
            };

            var response = await _httpClient.PostAsJsonAsync(QuotesEndpoint, request, ct);
            await EnsureSuccessAsync(response, "Create Quote");

            var result = await response.Content.ReadFromJsonAsync<WiseQuoteResponse>(cancellationToken: ct);
            return result?.Id ?? throw new InvalidOperationException("Failed to retrieve Quote ID from Wise response.");
        }

        private async Task<(string TransferId, string Status, decimal Fee, string FeeCurrency, DateTimeOffset? EstimatedDelivery)>
            CreateTransferAsync(Payout payout, string quoteId, string idempotencyKey, CancellationToken ct)
        {
            var request = new
            {
                targetAccount = payout.VendorId.ToString(),
                quoteUuid = quoteId,
                customerTransactionId = idempotencyKey,
                details = new
                {
                    reference = $"Payout-{payout.Id.ToString()[..8]}",
                    transferPurpose = "service_payment",
                    sourceOfFunds = "verification.source_of_funds.other"
                }
            };

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, TransfersEndpoint)
            {
                Content = JsonContent.Create(request)
            };

            var response = await _httpClient.SendAsync(requestMessage, ct);
            await EnsureSuccessAsync(response, "Create Transfer");

            var result = await response.Content.ReadFromJsonAsync<WiseTransferDetailResponse>(cancellationToken: ct);
            if (result is null)
                throw new InvalidOperationException("Failed to parse Transfer response from Wise.");

            return (
                result.Id.ToString(),
                result.Status ?? "processing",
                result.Fee ?? 0m,
                result.FeeCurrency ?? payout.Amount.Currency.Code,
                result.EstimatedDelivery
            );
        }

        private async Task EnsureSuccessAsync(HttpResponseMessage response, string action)
        {
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogError("Wise API {Action} failed. Status: {StatusCode}, Response: {Content}", action, response.StatusCode, content);
                throw new InvalidOperationException($"Wise API {action} failed with status {response.StatusCode}. Details: {content}");
            }
        }

        private class WiseQuoteResponse
        {
            [JsonPropertyName("id")]
            public string? Id { get; set; }
        }

        private class WiseTransferDetailResponse
        {
            [JsonPropertyName("id")]
            public long Id { get; set; }

            [JsonPropertyName("status")]
            public string? Status { get; set; }

            [JsonPropertyName("fee")]
            public decimal? Fee { get; set; }

            [JsonPropertyName("feeCurrency")]
            public string? FeeCurrency { get; set; }

            [JsonPropertyName("estimatedDelivery")]
            public DateTimeOffset? EstimatedDelivery { get; set; }
        }
    }
}
