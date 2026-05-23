using EnterpriseMediator.Contracts.Common;
using EnterpriseMediator.Contracts.DTOs.Common;

namespace Emp.ApiGateway.Application.Interfaces.Infrastructure;

/// <summary>
/// Contract for communicating with the audit log subsystem.
/// Handles retrieval and export of system audit trails.
/// </summary>
public interface IAuditServiceClient
{
    /// <summary>
    /// Retrieves a paginated list of audit log entries with optional filters.
    /// </summary>
    Task<PagedResultDto<AuditLogDto>> GetAuditLogsAsync(int page, int pageSize, string? userId, string? action, string? entityType, string? entityId, string? startDate, string? endDate, CancellationToken ct);

    /// <summary>
    /// Retrieves a single audit log entry by its unique identifier.
    /// </summary>
    Task<AuditLogDto?> GetAuditLogByIdAsync(Guid id, CancellationToken ct);

    /// <summary>
    /// Exports audit log entries as a CSV stream for the given filters.
    /// </summary>
    Task<Stream> ExportAuditLogsCsvAsync(string? userId, string? action, string? entityType, string? startDate, string? endDate, CancellationToken ct);
}
