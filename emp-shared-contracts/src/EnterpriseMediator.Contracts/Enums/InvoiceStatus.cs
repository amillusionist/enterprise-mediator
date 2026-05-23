using System.Text.Json.Serialization;

namespace EnterpriseMediator.Contracts.Enums;

/// <summary>
/// Invoice lifecycle states.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum InvoiceStatus
{
    Draft = 0,
    Sent = 1,
    Paid = 2,
    Overdue = 3,
    Cancelled = 4
}
