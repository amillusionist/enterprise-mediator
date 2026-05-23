using System.ComponentModel.DataAnnotations;

namespace EnterpriseMediator.Contracts.DTOs.Clients;

/// <summary>
/// Request payload for creating a new client company profile.
/// </summary>
public record CreateClientRequest
{
    [Required]
    public required string CompanyName { get; init; }
    public string? Address { get; init; }
    public required ContactInfo[] Contacts { get; init; }
}

/// <summary>
/// Contact information for a client company representative.
/// </summary>
public record ContactInfo
{
    [Required]
    public required string Name { get; init; }
    [Required, EmailAddress]
    public required string Email { get; init; }
    public string? Phone { get; init; }
}
