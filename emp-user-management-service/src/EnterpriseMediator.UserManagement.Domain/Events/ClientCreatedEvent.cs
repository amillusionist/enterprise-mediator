using System;
using MediatR;

namespace EnterpriseMediator.UserManagement.Domain.Events;

/// <summary>
/// Event raised when a new client organization is created.
/// </summary>
public record ClientCreatedEvent : INotification
{
    public Guid ClientId { get; }
    public string CompanyName { get; }
    public string PrimaryContactEmail { get; }
    public DateTimeOffset CreatedAt { get; }

    public ClientCreatedEvent(Guid clientId, string companyName, string primaryContactEmail, DateTimeOffset createdAt)
    {
        if (clientId == Guid.Empty) throw new ArgumentException("Client ID cannot be empty", nameof(clientId));

        ClientId = clientId;
        CompanyName = companyName;
        PrimaryContactEmail = primaryContactEmail;
        CreatedAt = createdAt;
    }
}
