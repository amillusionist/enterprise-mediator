using System.Text.Json.Serialization;

namespace EnterpriseMediator.Contracts.Enums;

/// <summary>
/// Project milestone approval states.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MilestoneStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Completed = 3
}
