using EnterpriseMediator.Contracts.Enums;

namespace EnterpriseMediator.Contracts.DTOs.Projects;

/// <summary>
/// Vendor proposal details for a project brief.
/// </summary>
public record ProposalDto
{
    public required Guid Id { get; init; }
    public required Guid ProjectId { get; init; }
    public required Guid VendorId { get; init; }
    public required string VendorName { get; init; }
    public required decimal Cost { get; init; }
    public required string Currency { get; init; }
    public required string Timeline { get; init; }
    public string[]? KeyPersonnel { get; init; }
    public required ProposalStatus Status { get; init; }
    public required DateTimeOffset SubmittedAt { get; init; }
    public DateTimeOffset? StatusChangedAt { get; init; }
    public string? InternalNotes { get; init; }
}
