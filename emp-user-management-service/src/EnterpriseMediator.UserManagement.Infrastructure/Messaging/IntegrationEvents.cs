namespace EnterpriseMediator.UserManagement.Infrastructure.Messaging;

/// <summary>
/// Integration event published to the message bus when a user is registered.
/// Consumed by notification service for welcome emails.
/// </summary>
public record UserRegisteredIntegrationEvent
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public DateTimeOffset RegisteredAt { get; init; }
}

/// <summary>
/// Integration event published when a vendor profile is updated.
/// Consumed by project service to update vector embeddings for matching.
/// </summary>
public record VendorProfileUpdatedIntegrationEvent
{
    public Guid VendorId { get; init; }
    public string[] Skills { get; init; } = Array.Empty<string>();
    public bool SkillsUpdated { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

/// <summary>
/// Integration event published when a vendor is created.
/// </summary>
public record VendorCreatedIntegrationEvent
{
    public Guid VendorId { get; init; }
    public string CompanyName { get; init; } = string.Empty;
    public string PrimaryContactEmail { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
}
