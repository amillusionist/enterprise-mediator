using System.Text.Json.Serialization;

namespace EnterpriseMediator.Contracts.Enums;

/// <summary>
/// Vendor payout processing states.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PayoutStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3
}
