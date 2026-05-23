using EnterpriseMediator.ProjectManagement.Domain.Enums;

namespace EnterpriseMediator.ProjectManagement.Domain.Aggregates.ProjectAggregate;

public class SowDocument
{
    public Guid Id { get; private set; }
    public Guid ProjectId { get; private set; }
    public string FileName { get; private set; } = null!;
    public string ContentType { get; private set; } = null!;
    public long FileSizeBytes { get; private set; }
    public string StorageKey { get; private set; } = null!;
    public SowDocumentStatus Status { get; private set; }
    public DateTime UploadedAt { get; private set; }
    public Guid UploadedBy { get; private set; }
    public string? FailureReason { get; private set; }

    private SowDocument() { }

    public SowDocument(
        Guid projectId,
        string fileName,
        string contentType,
        long fileSizeBytes,
        string storageKey,
        Guid uploadedBy)
    {
        if (projectId == Guid.Empty) throw new ArgumentException("Project ID cannot be empty.", nameof(projectId));
        if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("File name is required.", nameof(fileName));
        if (string.IsNullOrWhiteSpace(storageKey)) throw new ArgumentException("Storage key is required.", nameof(storageKey));

        Id = Guid.NewGuid();
        ProjectId = projectId;
        FileName = fileName;
        ContentType = contentType;
        FileSizeBytes = fileSizeBytes;
        StorageKey = storageKey;
        UploadedBy = uploadedBy;
        Status = SowDocumentStatus.Pending;
        UploadedAt = DateTime.UtcNow;
    }

    public void MarkProcessing()
    {
        if (Status != SowDocumentStatus.Pending)
            throw new InvalidOperationException($"Cannot start processing from {Status} state.");
        Status = SowDocumentStatus.Processing;
    }

    public void MarkProcessed()
    {
        if (Status != SowDocumentStatus.Processing)
            throw new InvalidOperationException($"Cannot mark processed from {Status} state.");
        Status = SowDocumentStatus.Processed;
    }

    public void MarkFailed(string reason)
    {
        Status = SowDocumentStatus.Failed;
        FailureReason = reason;
    }
}
