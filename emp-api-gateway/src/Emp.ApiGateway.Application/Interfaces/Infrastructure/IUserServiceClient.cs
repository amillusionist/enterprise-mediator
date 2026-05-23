using EnterpriseMediator.Contracts.Common;
using EnterpriseMediator.Contracts.DTOs.Clients;
using EnterpriseMediator.Contracts.DTOs.Users;
using EnterpriseMediator.Contracts.DTOs.Vendors;

namespace Emp.ApiGateway.Application.Interfaces.Infrastructure;

/// <summary>
/// Contract for communicating with the downstream User Management Microservice.
/// Handles user profile retrieval, invitations, activation, and vendor/client management.
/// </summary>
public interface IUserServiceClient
{
    /// <summary>
    /// Retrieves the user profile by their unique ID (Cognito sub).
    /// </summary>
    Task<UserProfileDto?> GetUserProfileAsync(Guid userId, CancellationToken ct);

    /// <summary>
    /// Retrieves the user profile by email address.
    /// </summary>
    Task<UserProfileDto?> GetUserProfileByEmailAsync(string email, CancellationToken ct);

    /// <summary>
    /// Sends an invitation to a new user with a specified role.
    /// </summary>
    Task<UserInvitationResultDto> InviteUserAsync(InviteUserDto dto, CancellationToken ct);

    /// <summary>
    /// Validates an invitation token and returns the invitation details.
    /// </summary>
    Task<UserInvitationDto?> ValidateInvitationAsync(string token, CancellationToken ct);

    /// <summary>
    /// Activates a user from an invitation token, setting their credentials.
    /// </summary>
    Task ActivateUserAsync(string token, ActivateUserRequest request, CancellationToken ct);

    // ──────────────────────────────────────────
    // Vendor Management
    // ──────────────────────────────────────────

    /// <summary>
    /// Retrieves a paginated list of vendors with optional filters.
    /// </summary>
    Task<PagedResultDto<VendorDto>> GetVendorsAsync(int page, int pageSize, string? search, string? status, string? skills, CancellationToken ct);

    /// <summary>
    /// Retrieves detailed vendor profile by ID.
    /// </summary>
    Task<VendorDetailDto?> GetVendorByIdAsync(Guid vendorId, CancellationToken ct);

    /// <summary>
    /// Creates a new vendor profile.
    /// </summary>
    Task<VendorDto> CreateVendorAsync(CreateVendorRequest request, CancellationToken ct);

    /// <summary>
    /// Updates an existing vendor profile.
    /// </summary>
    Task<VendorDto> UpdateVendorAsync(Guid vendorId, UpdateVendorRequest request, CancellationToken ct);

    /// <summary>
    /// Activates a vendor, allowing them to receive project briefs.
    /// </summary>
    Task ActivateVendorAsync(Guid vendorId, CancellationToken ct);

    /// <summary>
    /// Deactivates a vendor, preventing them from receiving project briefs.
    /// </summary>
    Task DeactivateVendorAsync(Guid vendorId, CancellationToken ct);

    /// <summary>
    /// Sends an invitation email to a vendor contact.
    /// </summary>
    Task InviteVendorContactAsync(Guid vendorId, string email, CancellationToken ct);

    // ──────────────────────────────────────────
    // Client Management
    // ──────────────────────────────────────────

    /// <summary>
    /// Retrieves a paginated list of clients with optional filters.
    /// </summary>
    Task<PagedResultDto<ClientDto>> GetClientsAsync(int page, int pageSize, string? search, string? status, CancellationToken ct);

    /// <summary>
    /// Retrieves a client profile by ID.
    /// </summary>
    Task<ClientDto?> GetClientByIdAsync(Guid clientId, CancellationToken ct);

    /// <summary>
    /// Creates a new client profile.
    /// </summary>
    Task<ClientDto> CreateClientAsync(CreateClientRequest request, CancellationToken ct);

    /// <summary>
    /// Updates an existing client profile.
    /// </summary>
    Task<ClientDto> UpdateClientAsync(Guid clientId, CreateClientRequest request, CancellationToken ct);

    /// <summary>
    /// Deactivates a client, preventing new project creation.
    /// </summary>
    Task DeactivateClientAsync(Guid clientId, CancellationToken ct);

    /// <summary>
    /// Reactivates a previously deactivated client.
    /// </summary>
    Task ReactivateClientAsync(Guid clientId, CancellationToken ct);
}

/// <summary>
/// Data Transfer Object for inviting a new user.
/// </summary>
public record InviteUserDto(string Email, string Role, Guid InvitedBy);

/// <summary>
/// Result of a user invitation request.
/// </summary>
public record UserInvitationResultDto
{
    public required string Email { get; init; }
    public required bool Success { get; init; }
    public string? InvitationId { get; init; }
    public string? ErrorMessage { get; init; }
}
