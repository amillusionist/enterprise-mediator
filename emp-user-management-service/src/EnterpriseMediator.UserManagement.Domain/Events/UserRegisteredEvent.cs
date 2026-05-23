using System;
using MediatR;

namespace EnterpriseMediator.UserManagement.Domain.Events;

/// <summary>
/// Event raised when a new user is registered in the system.
/// Triggers downstream processes such as welcome email workflows.
/// </summary>
public record UserRegisteredEvent : INotification
{
    public Guid UserId { get; }
    public string Email { get; }
    public string UserType { get; }
    public DateTimeOffset RegisteredAt { get; }

    public UserRegisteredEvent(Guid userId, string email, string userType, DateTimeOffset registeredAt)
    {
        if (userId == Guid.Empty) throw new ArgumentException("User ID cannot be empty", nameof(userId));
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email cannot be empty", nameof(email));
        if (string.IsNullOrWhiteSpace(userType)) throw new ArgumentException("User type cannot be empty", nameof(userType));

        UserId = userId;
        Email = email;
        UserType = userType;
        RegisteredAt = registeredAt;
    }
}
