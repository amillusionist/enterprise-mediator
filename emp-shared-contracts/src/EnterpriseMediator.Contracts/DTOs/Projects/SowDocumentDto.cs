namespace EnterpriseMediator.Contracts.DTOs.Projects;

/// <summary>
/// SOW document metadata and processing status.
/// </summary>
public record SowDocumentDto
{
    public required Guid Id { get; init; }
    public required Guid ProjectId { get; init; }
    public required string OriginalFileName { get; init; }
    public string? FileUrl { get; init; }
    public required string Status { get; init; }
    public string? UploadedBy { get; init; }
    public required DateTimeOffset UploadedAt { get; init; }
    public DateTimeOffset? ProcessedAt { get; init; }
    public string? ErrorDetails { get; init; }
}
