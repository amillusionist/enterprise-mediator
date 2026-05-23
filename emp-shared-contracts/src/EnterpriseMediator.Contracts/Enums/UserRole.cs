using System.Text.Json.Serialization;

namespace EnterpriseMediator.Contracts.Enums;

/// <summary>
/// System user roles mapped from AWS Cognito groups.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UserRole
{
    SystemAdministrator = 0,
    VendorContact = 1,
    ClientContact = 2
}
