using EnterpriseMediator.Financial.Application.DTOs;
using EnterpriseMediator.Financial.Application.Features.Invoices.Commands.GenerateInvoice;
using EnterpriseMediator.Financial.Application.Features.Invoices.Queries.GetInvoiceById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseMediator.Financial.Web.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[Produces("application/json")]
public class InvoicesController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ILogger<InvoicesController> _logger;

    public InvoicesController(ISender sender, ILogger<InvoicesController> logger)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Generates a client invoice for a specific project.
    /// </summary>
    /// <response code="201">Invoice successfully created.</response>
    /// <response code="400">Invalid request data.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="409">Conflict (invoice already exists for project).</response>
    [HttpPost]
    [Authorize(Roles = "SystemAdministrator,FinanceManager")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> GenerateInvoice(
        [FromBody] GenerateInvoiceCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received request to generate invoice for Project {ProjectId}", command.ProjectId);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            _logger.LogWarning("Invoice generation failed for Project {ProjectId}: {Error}", command.ProjectId, result.Error);
            return Conflict(new { error = result.Error });
        }

        _logger.LogInformation("Invoice {InvoiceId} generated for Project {ProjectId}", result.Value, command.ProjectId);
        return CreatedAtAction(nameof(GetInvoice), new { id = result.Value }, result.Value);
    }

    /// <summary>
    /// Retrieves invoice details by ID.
    /// </summary>
    /// <response code="200">Invoice found.</response>
    /// <response code="404">Invoice not found.</response>
    [HttpGet("{id}")]
    [Authorize(Roles = "SystemAdministrator,FinanceManager")]
    [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInvoice(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetInvoiceByIdQuery(id), cancellationToken);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }
}
