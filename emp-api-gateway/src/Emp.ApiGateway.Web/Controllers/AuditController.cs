using System.Net.Mime;
using Emp.ApiGateway.Application.Interfaces.Infrastructure;
using EnterpriseMediator.Contracts.Common;
using EnterpriseMediator.Contracts.DTOs.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Emp.ApiGateway.Web.Controllers;

/// <summary>
/// Provides access to the system audit trail.
/// Supports paginated listing, detail retrieval, and CSV export of audit log entries.
/// </summary>
[ApiController]
[Route("api/v1/audit")]
[Authorize]
[Produces(MediaTypeNames.Application.Json)]
public class AuditController : ControllerBase
{
    private readonly IAuditServiceClient _auditService;
    private readonly ILogger<AuditController> _logger;

    public AuditController(IAuditServiceClient auditService, ILogger<AuditController> logger)
    {
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves a paginated list of audit log entries with optional filters.
    /// </summary>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="userId">Optional user ID filter.</param>
    /// <param name="action">Optional action type filter.</param>
    /// <param name="entityType">Optional entity type filter.</param>
    /// <param name="entityId">Optional entity ID filter.</param>
    /// <param name="startDate">Optional start date filter (ISO 8601).</param>
    /// <param name="endDate">Optional end date filter (ISO 8601).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Paginated list of audit log entries.</returns>
    /// <response code="200">Audit logs retrieved successfully.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<AuditLogDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResultDto<AuditLogDto>>> GetAuditLogs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? userId = null,
        [FromQuery] string? action = null,
        [FromQuery] string? entityType = null,
        [FromQuery] string? entityId = null,
        [FromQuery] string? startDate = null,
        [FromQuery] string? endDate = null,
        CancellationToken ct = default)
    {
        var result = await _auditService.GetAuditLogsAsync(
            page, pageSize, userId, action, entityType, entityId, startDate, endDate, ct);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a single audit log entry by its unique identifier.
    /// </summary>
    /// <param name="id">The audit log entry identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Audit log entry details.</returns>
    /// <response code="200">Audit log entry retrieved.</response>
    /// <response code="404">Audit log entry not found.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AuditLogDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AuditLogDto>> GetAuditLogById(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        var result = await _auditService.GetAuditLogByIdAsync(id, ct);

        if (result == null)
        {
            _logger.LogWarning("Audit log entry not found for ID: {AuditLogId}", id);
            return NotFound($"Audit log entry with ID {id} not found.");
        }

        return Ok(result);
    }

    /// <summary>
    /// Exports audit log entries as a CSV file for the given filters.
    /// </summary>
    /// <param name="userId">Optional user ID filter.</param>
    /// <param name="action">Optional action type filter.</param>
    /// <param name="entityType">Optional entity type filter.</param>
    /// <param name="startDate">Optional start date filter (ISO 8601).</param>
    /// <param name="endDate">Optional end date filter (ISO 8601).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>CSV file stream.</returns>
    /// <response code="200">CSV export generated.</response>
    [HttpGet("export")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportAuditLogsCsv(
        [FromQuery] string? userId = null,
        [FromQuery] string? action = null,
        [FromQuery] string? entityType = null,
        [FromQuery] string? startDate = null,
        [FromQuery] string? endDate = null,
        CancellationToken ct = default)
    {
        var csvStream = await _auditService.ExportAuditLogsCsvAsync(userId, action, entityType, startDate, endDate, ct);
        return File(csvStream, "text/csv", "audit-logs.csv");
    }
}
