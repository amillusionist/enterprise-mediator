using System.Text.Json.Serialization;

namespace EnterpriseMediator.Contracts.Enums;

/// <summary>
/// Project lifecycle states. Serialized as string for frontend readability.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ProjectStatus
{
    /// <summary>Project created, awaiting SOW upload or processing.</summary>
    Pending = 0,

    /// <summary>SOW is being processed by AI extraction pipeline.</summary>
    SowProcessing = 1,

    /// <summary>AI extraction complete, brief ready for human review.</summary>
    ReviewPending = 2,

    /// <summary>Brief approved, ready for vendor matching and proposal solicitation.</summary>
    Proposed = 3,

    /// <summary>Vendor selected, project awarded.</summary>
    Awarded = 4,

    /// <summary>Project actively in progress.</summary>
    Active = 5,

    /// <summary>Project temporarily paused.</summary>
    OnHold = 6,

    /// <summary>Project successfully completed.</summary>
    Completed = 7,

    /// <summary>Project cancelled.</summary>
    Cancelled = 8
}
