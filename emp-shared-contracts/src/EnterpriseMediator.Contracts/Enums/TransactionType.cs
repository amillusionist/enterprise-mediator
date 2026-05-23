using System.Text.Json.Serialization;

namespace EnterpriseMediator.Contracts.Enums;

/// <summary>
/// Financial transaction types for the immutable ledger.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TransactionType
{
    InvoicePayment = 0,
    VendorPayout = 1,
    Refund = 2,
    PlatformFee = 3,
    Adjustment = 4
}
