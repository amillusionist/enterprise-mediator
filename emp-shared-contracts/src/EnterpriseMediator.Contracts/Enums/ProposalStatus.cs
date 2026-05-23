using System.Text.Json.Serialization;

namespace EnterpriseMediator.Contracts.Enums;

/// <summary>
/// Vendor proposal lifecycle states.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ProposalStatus
{
    Submitted = 0,
    InReview = 1,
    Shortlisted = 2,
    Accepted = 3,
    Rejected = 4
}
