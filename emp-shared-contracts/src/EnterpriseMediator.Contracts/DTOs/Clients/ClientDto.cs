namespace EnterpriseMediator.Contracts.DTOs.Clients;

/// <summary>
/// Client company profile response.
/// </summary>
public record ClientDto
{
    public required Guid Id { get; init; }
    public required string CompanyName { get; init; }
    public required string Status { get; init; }
    public string? PrimaryContactName { get; init; }
    public string? PrimaryContactEmail { get; init; }
    public int ActiveProjectsCount { get; init; }
    public string? Address { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}
