namespace EnterpriseMediator.AiWorker.Application.Interfaces;

/// <summary>
/// Repository interface for managing SOW document entities during processing.
/// </summary>
public interface ISowRepository
{
    /// <summary>
    /// Retrieves a SOW entity by its unique identifier.
    /// </summary>
    Task<SowEntity?> GetByIdAsync(Guid sowId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists changes to a SOW entity.
    /// </summary>
    Task UpdateAsync(SowEntity entity, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents the persisted SOW document entity used during processing.
/// </summary>
public class SowEntity
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? SanitizedContent { get; set; }
    public string? ExtractedDataJson { get; set; }
    public float[]? VectorEmbeddings { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
}
