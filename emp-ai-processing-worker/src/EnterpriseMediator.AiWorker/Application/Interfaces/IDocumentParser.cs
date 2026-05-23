namespace EnterpriseMediator.AiWorker.Application.Interfaces;

/// <summary>
/// Defines the contract for parsing text content from document files.
/// Implementations should support common document formats (.pdf, .docx, .doc).
/// </summary>
public interface IDocumentParser
{
    /// <summary>
    /// Extracts plain text from the given document stream.
    /// </summary>
    /// <param name="fileStream">The raw file stream of the document.</param>
    /// <param name="fileExtension">The file extension (e.g., ".pdf", ".docx") to determine parsing strategy.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The extracted plain text content.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the file format is unsupported or parsing fails.</exception>
    Task<string> ParseTextAsync(Stream fileStream, string fileExtension, CancellationToken cancellationToken = default);
}
