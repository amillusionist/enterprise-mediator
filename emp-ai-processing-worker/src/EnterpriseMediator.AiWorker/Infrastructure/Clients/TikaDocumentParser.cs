using EnterpriseMediator.AiWorker.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace EnterpriseMediator.AiWorker.Infrastructure.Clients;

/// <summary>
/// Parses document text using Apache Tika (via HTTP server).
/// Supports PDF, DOCX, and DOC formats commonly used for SOW documents.
/// </summary>
public class TikaDocumentParser : IDocumentParser
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TikaDocumentParser> _logger;

    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".docx", ".doc"
    };

    public TikaDocumentParser(HttpClient httpClient, ILogger<TikaDocumentParser> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> ParseTextAsync(Stream fileStream, string fileExtension, CancellationToken cancellationToken = default)
    {
        if (fileStream == null) throw new ArgumentNullException(nameof(fileStream));
        if (string.IsNullOrWhiteSpace(fileExtension)) throw new ArgumentException("File extension is required.", nameof(fileExtension));

        if (!SupportedExtensions.Contains(fileExtension))
        {
            throw new InvalidOperationException($"Unsupported file format: {fileExtension}. Supported formats: {string.Join(", ", SupportedExtensions)}");
        }

        _logger.LogDebug("Parsing document with extension {FileExtension} via Tika", fileExtension);

        using var content = new StreamContent(fileStream);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(GetMimeType(fileExtension));

        var request = new HttpRequestMessage(HttpMethod.Put, "/tika")
        {
            Content = content
        };
        request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/plain"));

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var text = await response.Content.ReadAsStringAsync(cancellationToken);

        _logger.LogInformation("Tika successfully parsed document. Extracted {CharCount} characters", text.Length);

        return text;
    }

    private static string GetMimeType(string extension) => extension.ToLowerInvariant() switch
    {
        ".pdf" => "application/pdf",
        ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        ".doc" => "application/msword",
        _ => "application/octet-stream"
    };
}
