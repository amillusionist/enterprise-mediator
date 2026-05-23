using EnterpriseMediator.Financial.Application.DTOs;
using EnterpriseMediator.Financial.Application.Features.Payouts.Commands.ApprovePayout;
using EnterpriseMediator.Financial.Application.Features.Payouts.Commands.InitiatePayout;
using EnterpriseMediator.Financial.Application.Features.Payouts.Commands.RejectPayout;
using EnterpriseMediator.Financial.Application.Features.Payouts.Queries.GetPendingPayouts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseMediator.Financial.Web.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "SystemAdministrator,FinanceManager")]
[Produces("application/json")]
public class PayoutsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ILogger<PayoutsController> _logger;

    public PayoutsController(ISender sender, ILogger<PayoutsController> logger)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Initiates a new payout request for a vendor.
    /// </summary>
    /// <response code="201">Payout initiated.</response>
    /// <response code="400">Invalid request.</response>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> InitiatePayout(
        [FromBody] InitiatePayoutCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Initiating payout for Vendor {VendorId} on Project {ProjectId}",
            command.VendorId, command.ProjectId);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetPendingPayouts), null, result.Value);
    }

    /// <summary>
    /// Retrieves all payouts awaiting approval.
    /// </summary>
    /// <response code="200">List of pending payouts.</response>
    [HttpGet("pending")]
    [ProducesResponseType(typeof(IReadOnlyList<PayoutDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPendingPayouts(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetPendingPayoutsQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Approves a pending payout and submits it to the payout gateway.
    /// </summary>
    /// <response code="200">Payout approved and submitted.</response>
    /// <response code="404">Payout not found.</response>
    [HttpPost("{id}/approve")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApprovePayout(
        Guid id,
        [FromBody] ApprovePayoutRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ApprovePayoutCommand(id, request.ApproverId);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(new { message = "Payout approved and submitted for processing." });
    }

    /// <summary>
    /// Rejects a pending payout.
    /// </summary>
    /// <response code="200">Payout rejected.</response>
    /// <response code="404">Payout not found.</response>
    [HttpPost("{id}/reject")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RejectPayout(
        Guid id,
        [FromBody] RejectPayoutRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RejectPayoutCommand(id, request.RejectorId, request.Reason);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(new { message = "Payout rejected." });
    }
}

public record ApprovePayoutRequest(Guid ApproverId);
public record RejectPayoutRequest(Guid RejectorId, string Reason);
