namespace EnterpriseMediator.Financial.Domain.Interfaces;

/// <summary>
/// Abstraction over system clock for testability.
/// </summary>
public interface IDateTime
{
    /// <summary>Gets the current UTC date and time.</summary>
    DateTime UtcNow { get; }

    /// <summary>Gets the current local date and time.</summary>
    DateTime Now { get; }

    /// <summary>Gets the current date (time portion set to 00:00:00).</summary>
    DateTime Today { get; }
}
