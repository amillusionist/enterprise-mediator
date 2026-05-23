using EnterpriseMediator.UserManagement.Application.Features.Clients.Queries.GetClientDetails;
using EnterpriseMediator.UserManagement.Application.Features.Internal.Queries.GetClientById;
using EnterpriseMediator.UserManagement.Application.Features.Internal.Queries.GetUserRole;
using EnterpriseMediator.UserManagement.Application.Features.Internal.Queries.GetVendorPaymentDetails;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseMediator.UserManagement.API.Controllers;

/// <summary>
/// Internal API controller exposing user management data to other microservices.
/// Protected by strict internal policies to prevent public access.
/// </summary>
[ApiController]
[Route("api/v1/internal")]
[Authorize(Policy = "InternalServicePolicy")]
public class InternalUsersController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ILogger<InternalUsersController> _logger;

    public InternalUsersController(ISender sender, ILogger<InternalUsersController> logger)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves the role of a user for RBAC checks in other services.
    /// </summary>
    [HttpGet("users/{id}/role")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<string>> GetUserRole(Guid id, CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
            return BadRequest("Invalid User ID.");

        _logger.LogDebug("Internal role lookup for user {UserId}", id);

        var result = await _sender.Send(new GetUserRoleQuery(id), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves client details for project association.
    /// Consumed by Project Management Service.
    /// </summary>
    [HttpGet("clients/{id}")]
    [ProducesResponseType(typeof(ClientDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientDetailsDto>> GetClient(Guid id, CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
            return BadRequest("Invalid Client ID.");

        _logger.LogDebug("Internal client lookup for {ClientId}", id);

        var result = await _sender.Send(new GetClientByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves vendor payment details for the Financial Service.
    /// Returns unmasked payment data - strictly secured endpoint.
    /// </summary>
    [HttpGet("vendors/{id}/payment-details")]
    [ProducesResponseType(typeof(PaymentDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentDetailsDto>> GetVendorPaymentDetails(Guid id, CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
            return BadRequest("Invalid Vendor ID.");

        _logger.LogDebug("Internal payment details lookup for vendor {VendorId}", id);

        var result = await _sender.Send(new GetVendorPaymentDetailsQuery(id), cancellationToken);
        return Ok(result);
    }
}
