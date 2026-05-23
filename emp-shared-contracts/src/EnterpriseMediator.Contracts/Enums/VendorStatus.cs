using System.Text.Json.Serialization;

namespace EnterpriseMediator.Contracts.Enums;

/// <summary>
/// Vendor profile lifecycle states.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum VendorStatus
{
    Pending = 0,
    Active = 1,
    Suspended = 2,
    Deactivated = 3
}
