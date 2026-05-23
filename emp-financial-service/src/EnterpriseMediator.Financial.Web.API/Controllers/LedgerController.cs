using EnterpriseMediator.Financial.Application.Features.Ledger.DTOs;
using EnterpriseMediator.Financial.Application.Features.Ledger.Queries.GetTransactionHistory;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseMediator.Financial.Web.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "SystemAdministrator,FinanceManager")]
[Produces("application/json")]
public class LedgerController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ILogger<LedgerController> _logger;

    public LedgerController(ISender sender, ILogger<LedgerController> logger)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves the financial transaction ledger with optional filtering and pagination.
    /// </summary>
    /// <response code="200">Returns the requested ledger data.</response>
    /// <response code="400">Invalid filter parameters.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<TransactionSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetTransactionHistory(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] string? type,
        [FromQuery] Guid? projectId,
        [FromQuery] Guid? clientId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        if (startDate.HasValue && endDate.HasValue && startDate > endDate)
            return BadRequest(new { error = "Start date cannot be after end date." });

        _logger.LogInformation("Retrieving transaction ledger. Page: {Page}, Size: {Size}, Project: {ProjectId}",
            page, pageSize, projectId);

        var query = new GetTransactionHistoryQuery
        {
            StartDate = startDate,
            EndDate = endDate,
            TransactionType = type,
            ProjectId = projectId,
            ClientId = clientId,
            Page = page,
            PageSize = pageSize
        };

        var result = await _sender.Send(query, cancellationToken);
        return Ok(result);
    }
}
