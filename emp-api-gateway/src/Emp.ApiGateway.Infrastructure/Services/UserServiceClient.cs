using System.Net;
using System.Net.Http.Json;
using Emp.ApiGateway.Application.Interfaces.Infrastructure;
using Emp.ApiGateway.Infrastructure.Configuration;
using EnterpriseMediator.Contracts.Common;
using EnterpriseMediator.Contracts.DTOs.Clients;
using EnterpriseMediator.Contracts.DTOs.Users;
using EnterpriseMediator.Contracts.DTOs.Vendors;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Emp.ApiGateway.Infrastructure.Services;

/// <summary>
/// HTTP Client implementation for communicating with the User Management Microservice.
/// Handles user profiles, invitations, and activation.
/// </summary>
public class UserServiceClient : IUserServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UserServiceClient> _logger;

    public UserServiceClient(
        HttpClient httpClient,
        IOptions<ServiceUrls> serviceUrls,
        ILogger<UserServiceClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        var urls = serviceUrls?.Value ?? throw new ArgumentNullException(nameof(serviceUrls));

        if (_httpClient.BaseAddress == null && !string.IsNullOrEmpty(urls.UserService))
        {
            _httpClient.BaseAddress = new Uri(urls.UserService);
        }
    }

    public async Task<UserProfileDto?> GetUserProfileAsync(Guid userId, CancellationToken ct)
    {
        _logger.LogInformation("Fetching user profile for UserId: {UserId}", userId);

        var response = await _httpClient.GetAsync($"api/v1/users/{userId}", ct);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<UserProfileDto>(cancellationToken: ct);
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("User service returned 404 for UserId: {UserId}", userId);
            return null;
        }

        var content = await response.Content.ReadAsStringAsync(ct);
        _logger.LogError("User service call failed. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, content);
        throw new HttpRequestException($"User service call failed with status code {response.StatusCode}");
    }

    public async Task<UserProfileDto?> GetUserProfileByEmailAsync(string email, CancellationToken ct)
    {
        _logger.LogInformation("Fetching user profile by email");

        var response = await _httpClient.GetAsync($"api/v1/users/by-email?email={Uri.EscapeDataString(email)}", ct);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<UserProfileDto>(cancellationToken: ct);
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        var content = await response.Content.ReadAsStringAsync(ct);
        _logger.LogError("User service call failed. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, content);
        throw new HttpRequestException($"User service call failed with status code {response.StatusCode}");
    }

    public async Task<UserInvitationResultDto> InviteUserAsync(InviteUserDto dto, CancellationToken ct)
    {
        _logger.LogInformation("Inviting user with email: {Email}, role: {Role}", dto.Email, dto.Role);

        var response = await _httpClient.PostAsJsonAsync("api/v1/users/invite", dto, ct);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<UserInvitationResultDto>(cancellationToken: ct);
            _logger.LogInformation("User invitation sent successfully for email: {Email}", dto.Email);
            return result!;
        }

        var content = await response.Content.ReadAsStringAsync(ct);
        _logger.LogError("Failed to invite user. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, content);
        throw new HttpRequestException($"Invite user failed with status code {response.StatusCode}: {content}");
    }

    public async Task<UserInvitationDto?> ValidateInvitationAsync(string token, CancellationToken ct)
    {
        _logger.LogInformation("Validating invitation token");

        var response = await _httpClient.GetAsync($"api/v1/users/invitations/{Uri.EscapeDataString(token)}/validate", ct);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<UserInvitationDto>(cancellationToken: ct);
        }

        if (response.StatusCode == HttpStatusCode.NotFound || response.StatusCode == HttpStatusCode.Gone)
        {
            _logger.LogWarning("Invitation token not found or expired");
            return null;
        }

        var content = await response.Content.ReadAsStringAsync(ct);
        _logger.LogError("Failed to validate invitation. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, content);
        throw new HttpRequestException($"Validate invitation failed with status code {response.StatusCode}");
    }

    public async Task ActivateUserAsync(string token, ActivateUserRequest request, CancellationToken ct)
    {
        _logger.LogInformation("Activating user from invitation token");

        var response = await _httpClient.PostAsJsonAsync($"api/v1/users/invitations/{Uri.EscapeDataString(token)}/activate", request, ct);

        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation("User activated successfully from invitation token");
            return;
        }

        var content = await response.Content.ReadAsStringAsync(ct);
        _logger.LogError("Failed to activate user. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, content);
        throw new HttpRequestException($"Activate user failed with status code {response.StatusCode}: {content}");
    }

    // ──────────────────────────────────────────
    // Vendor Management
    // ──────────────────────────────────────────

    public async Task<PagedResultDto<VendorDto>> GetVendorsAsync(int page, int pageSize, string? search, string? status, string? skills, CancellationToken ct)
    {
        _logger.LogInformation("Fetching vendors. Page: {Page}, PageSize: {PageSize}", page, pageSize);

        var queryParams = $"?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrEmpty(search)) queryParams += $"&search={Uri.EscapeDataString(search)}";
        if (!string.IsNullOrEmpty(status)) queryParams += $"&status={Uri.EscapeDataString(status)}";
        if (!string.IsNullOrEmpty(skills)) queryParams += $"&skills={Uri.EscapeDataString(skills)}";

        var response = await _httpClient.GetAsync($"api/v1/vendors{queryParams}", ct);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<PagedResultDto<VendorDto>>(cancellationToken: ct)
                   ?? new PagedResultDto<VendorDto> { Items = [], TotalCount = 0, Page = page, PageSize = pageSize };
        }

        var content = await response.Content.ReadAsStringAsync(ct);
        _logger.LogError("Failed to fetch vendors. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, content);
        throw new HttpRequestException($"Get vendors failed with status code {response.StatusCode}");
    }

    public async Task<VendorDetailDto?> GetVendorByIdAsync(Guid vendorId, CancellationToken ct)
    {
        _logger.LogInformation("Fetching vendor profile for VendorId: {VendorId}", vendorId);

        var response = await _httpClient.GetAsync($"api/v1/vendors/{vendorId}", ct);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<VendorDetailDto>(cancellationToken: ct);
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("User service returned 404 for VendorId: {VendorId}", vendorId);
            return null;
        }

        var content = await response.Content.ReadAsStringAsync(ct);
        _logger.LogError("Failed to fetch vendor. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, content);
        throw new HttpRequestException($"Get vendor failed with status code {response.StatusCode}");
    }

    public async Task<VendorDto> CreateVendorAsync(CreateVendorRequest request, CancellationToken ct)
    {
        _logger.LogInformation("Creating vendor with company name: {CompanyName}", request.CompanyName);

        var response = await _httpClient.PostAsJsonAsync("api/v1/vendors", request, ct);

        if (response.IsSuccessStatusCode)
        {
            var vendor = await response.Content.ReadFromJsonAsync<VendorDto>(cancellationToken: ct);
            _logger.LogInformation("Vendor created successfully: {VendorId}", vendor!.Id);
            return vendor;
        }

        var content = await response.Content.ReadAsStringAsync(ct);
        _logger.LogError("Failed to create vendor. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, content);
        throw new HttpRequestException($"Create vendor failed with status code {response.StatusCode}: {content}");
    }

    public async Task<VendorDto> UpdateVendorAsync(Guid vendorId, UpdateVendorRequest request, CancellationToken ct)
    {
        _logger.LogInformation("Updating vendor: {VendorId}", vendorId);

        var response = await _httpClient.PutAsJsonAsync($"api/v1/vendors/{vendorId}", request, ct);

        if (response.IsSuccessStatusCode)
        {
            var vendor = await response.Content.ReadFromJsonAsync<VendorDto>(cancellationToken: ct);
            _logger.LogInformation("Vendor updated successfully: {VendorId}", vendor!.Id);
            return vendor;
        }

        var content = await response.Content.ReadAsStringAsync(ct);
        _logger.LogError("Failed to update vendor. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, content);
        throw new HttpRequestException($"Update vendor failed with status code {response.StatusCode}: {content}");
    }

    public async Task ActivateVendorAsync(Guid vendorId, CancellationToken ct)
    {
        _logger.LogInformation("Activating vendor: {VendorId}", vendorId);

        var response = await _httpClient.PostAsync($"api/v1/vendors/{vendorId}/activate", null, ct);

        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Failed to activate vendor. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, content);
            throw new HttpRequestException($"Activate vendor failed with status code {response.StatusCode}: {content}");
        }

        _logger.LogInformation("Vendor activated successfully: {VendorId}", vendorId);
    }

    public async Task DeactivateVendorAsync(Guid vendorId, CancellationToken ct)
    {
        _logger.LogInformation("Deactivating vendor: {VendorId}", vendorId);

        var response = await _httpClient.PostAsync($"api/v1/vendors/{vendorId}/deactivate", null, ct);

        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Failed to deactivate vendor. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, content);
            throw new HttpRequestException($"Deactivate vendor failed with status code {response.StatusCode}: {content}");
        }

        _logger.LogInformation("Vendor deactivated successfully: {VendorId}", vendorId);
    }

    public async Task InviteVendorContactAsync(Guid vendorId, string email, CancellationToken ct)
    {
        _logger.LogInformation("Inviting vendor contact for VendorId: {VendorId}, Email: {Email}", vendorId, email);

        var response = await _httpClient.PostAsJsonAsync($"api/v1/vendors/{vendorId}/invite", new { Email = email }, ct);

        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Failed to invite vendor contact. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, content);
            throw new HttpRequestException($"Invite vendor contact failed with status code {response.StatusCode}: {content}");
        }

        _logger.LogInformation("Vendor contact invitation sent successfully for VendorId: {VendorId}", vendorId);
    }

    // ──────────────────────────────────────────
    // Client Management
    // ──────────────────────────────────────────

    public async Task<PagedResultDto<ClientDto>> GetClientsAsync(int page, int pageSize, string? search, string? status, CancellationToken ct)
    {
        _logger.LogInformation("Fetching clients. Page: {Page}, PageSize: {PageSize}", page, pageSize);

        var queryParams = $"?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrEmpty(search)) queryParams += $"&search={Uri.EscapeDataString(search)}";
        if (!string.IsNullOrEmpty(status)) queryParams += $"&status={Uri.EscapeDataString(status)}";

        var response = await _httpClient.GetAsync($"api/v1/clients{queryParams}", ct);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<PagedResultDto<ClientDto>>(cancellationToken: ct)
                   ?? new PagedResultDto<ClientDto> { Items = [], TotalCount = 0, Page = page, PageSize = pageSize };
        }

        var content = await response.Content.ReadAsStringAsync(ct);
        _logger.LogError("Failed to fetch clients. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, content);
        throw new HttpRequestException($"Get clients failed with status code {response.StatusCode}");
    }

    public async Task<ClientDto?> GetClientByIdAsync(Guid clientId, CancellationToken ct)
    {
        _logger.LogInformation("Fetching client profile for ClientId: {ClientId}", clientId);

        var response = await _httpClient.GetAsync($"api/v1/clients/{clientId}", ct);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<ClientDto>(cancellationToken: ct);
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("User service returned 404 for ClientId: {ClientId}", clientId);
            return null;
        }

        var content = await response.Content.ReadAsStringAsync(ct);
        _logger.LogError("Failed to fetch client. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, content);
        throw new HttpRequestException($"Get client failed with status code {response.StatusCode}");
    }

    public async Task<ClientDto> CreateClientAsync(CreateClientRequest request, CancellationToken ct)
    {
        _logger.LogInformation("Creating client with company name: {CompanyName}", request.CompanyName);

        var response = await _httpClient.PostAsJsonAsync("api/v1/clients", request, ct);

        if (response.IsSuccessStatusCode)
        {
            var client = await response.Content.ReadFromJsonAsync<ClientDto>(cancellationToken: ct);
            _logger.LogInformation("Client created successfully: {ClientId}", client!.Id);
            return client;
        }

        var content = await response.Content.ReadAsStringAsync(ct);
        _logger.LogError("Failed to create client. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, content);
        throw new HttpRequestException($"Create client failed with status code {response.StatusCode}: {content}");
    }

    public async Task<ClientDto> UpdateClientAsync(Guid clientId, CreateClientRequest request, CancellationToken ct)
    {
        _logger.LogInformation("Updating client: {ClientId}", clientId);

        var response = await _httpClient.PutAsJsonAsync($"api/v1/clients/{clientId}", request, ct);

        if (response.IsSuccessStatusCode)
        {
            var client = await response.Content.ReadFromJsonAsync<ClientDto>(cancellationToken: ct);
            _logger.LogInformation("Client updated successfully: {ClientId}", client!.Id);
            return client;
        }

        var content = await response.Content.ReadAsStringAsync(ct);
        _logger.LogError("Failed to update client. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, content);
        throw new HttpRequestException($"Update client failed with status code {response.StatusCode}: {content}");
    }

    public async Task DeactivateClientAsync(Guid clientId, CancellationToken ct)
    {
        _logger.LogInformation("Deactivating client: {ClientId}", clientId);

        var response = await _httpClient.PostAsync($"api/v1/clients/{clientId}/deactivate", null, ct);

        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Failed to deactivate client. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, content);
            throw new HttpRequestException($"Deactivate client failed with status code {response.StatusCode}: {content}");
        }

        _logger.LogInformation("Client deactivated successfully: {ClientId}", clientId);
    }

    public async Task ReactivateClientAsync(Guid clientId, CancellationToken ct)
    {
        _logger.LogInformation("Reactivating client: {ClientId}", clientId);

        var response = await _httpClient.PostAsync($"api/v1/clients/{clientId}/reactivate", null, ct);

        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Failed to reactivate client. StatusCode: {StatusCode}, Content: {Content}", response.StatusCode, content);
            throw new HttpRequestException($"Reactivate client failed with status code {response.StatusCode}: {content}");
        }

        _logger.LogInformation("Client reactivated successfully: {ClientId}", clientId);
    }
}
