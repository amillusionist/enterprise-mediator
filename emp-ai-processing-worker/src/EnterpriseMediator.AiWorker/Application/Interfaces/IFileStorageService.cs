namespace EnterpriseMediator.AiWorker.Application.Interfaces;

/// <summary>
/// Defines the contract for accessing file storage (e.g., AWS S3).
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Retrieves a file stream from storage by its key.
    /// </summary>
    /// <param name="fileKey">The storage key (S3 object key) for the file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A readable stream of the file content.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the file does not exist in storage.</exception>
    Task<Stream> GetFileStreamAsync(string fileKey, CancellationToken cancellationToken = default);
}
