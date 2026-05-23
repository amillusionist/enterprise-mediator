namespace EnterpriseMediator.Core.SharedKernel.Configuration;

/// <summary>
/// Configuration settings for structured logging via Serilog.
/// Bound from the "SharedKernel:Serilog" configuration section.
/// </summary>
public class SerilogOptions
{
    /// <summary>
    /// The minimum log level to record (e.g., Information, Warning, Error).
    /// </summary>
    public string MinimumLevel { get; set; } = "Information";

    /// <summary>
    /// Whether to enable console sink output.
    /// </summary>
    public bool UseConsole { get; set; } = true;

    /// <summary>
    /// Whether to enable rolling file sink output.
    /// </summary>
    public bool UseFile { get; set; } = false;

    /// <summary>
    /// The path for log files when file logging is enabled.
    /// </summary>
    public string LogFilePath { get; set; } = "logs/log-.txt";

    /// <summary>
    /// Whether to send logs to a Seq server for centralized aggregation.
    /// </summary>
    public bool UseSeq { get; set; } = false;

    /// <summary>
    /// The URL of the Seq server (e.g., "http://localhost:5341").
    /// </summary>
    public string? SeqUrl { get; set; }

    /// <summary>
    /// Whether to send logs to Elasticsearch.
    /// </summary>
    public bool UseElasticsearch { get; set; } = false;

    /// <summary>
    /// The URL of the Elasticsearch cluster for log ingestion.
    /// </summary>
    public string? ElasticsearchUrl { get; set; }

    /// <summary>
    /// The application name to enrich logs with.
    /// </summary>
    public string ApplicationName { get; set; } = "EnterpriseMediator";
}