using EnterpriseMediator.Financial.Application.DTOs;
using EnterpriseMediator.Financial.Application.Features.Financials.Queries.GetFinancialSummary;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseMediator.Financial.Web.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[Produces("application/json")]
public class FinancialsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ILogger<FinancialsController> _logger;

    public FinancialsController(ISender sender, ILogger<FinancialsController> logger)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves financial summary for a specific project.
    /// Used by the API Gateway for dashboard aggregation.
    /// </summary>
    /// <response code="200">Financial summary.</response>
    /// <response code="404">Project financial data not found.</response>
    [HttpGet("projects/{projectId}/summary")]
    [ProducesResponseType(typeof(FinancialSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProjectFinancialSummary(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving financial summary for Project {ProjectId}", projectId);

        var result = await _sender.Send(new GetFinancialSummaryQuery(projectId), cancellationToken);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }
}
