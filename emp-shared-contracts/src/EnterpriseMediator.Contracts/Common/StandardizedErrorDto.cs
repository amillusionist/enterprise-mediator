namespace EnterpriseMediator.Contracts.Common;

/// <summary>
/// Uniform error structure for all API responses. Used by global exception handlers
/// to ensure consistent error parsing on the frontend.
/// </summary>
public record StandardizedErrorDto
{
    /// <summary>
    /// Correlation/trace ID from the request context.
    /// </summary>
    public string? TraceId { get; init; }

    /// <summary>
    /// Machine-readable error code (e.g., "VALIDATION_ERROR", "NOT_FOUND").
    /// </summary>
    public required string Code { get; init; }

    /// <summary>
    /// Human-readable error message.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Field-level validation errors. Key is the property name, value is array of error messages.
    /// </summary>
    public IDictionary<string, string[]>? Details { get; init; }
}
