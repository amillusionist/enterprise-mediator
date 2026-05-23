using EnterpriseMediator.UserManagement.Application.Features.Vendors.Commands.CreateVendor;
using EnterpriseMediator.UserManagement.Application.Features.Vendors.Commands.UpdateProfile;
using EnterpriseMediator.UserManagement.Application.Features.Vendors.Queries.GetVendorDetails;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseMediator.UserManagement.API.Controllers;

/// <summary>
/// Manages Vendor entity operations including onboarding and profile management.
/// </summary>
[ApiController]
[Route("api/v1/vendors")]
[Authorize]
public class VendorsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ILogger<VendorsController> _logger;

    public VendorsController(ISender sender, ILogger<VendorsController> logger)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new vendor profile.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "SystemAdmin")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Guid>> CreateVendor(
        [FromBody] CreateVendorCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating new vendor: {CompanyName}", command.CompanyName);

        var result = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetVendor), new { id = result }, result);
    }

    /// <summary>
    /// Retrieves details for a specific vendor.
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "SystemAdmin,VendorContact")]
    [ProducesResponseType(typeof(VendorDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VendorDetailsDto>> GetVendor(Guid id, CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
            return BadRequest("Invalid Vendor ID.");

        var result = await _sender.Send(new GetVendorDetailsQuery(id), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Updates an existing vendor's profile information.
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "SystemAdmin,VendorContact")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProfile(
        Guid id,
        [FromBody] UpdateVendorProfileCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.VendorId)
            return BadRequest("Route ID does not match command ID.");

        _logger.LogInformation("Updating profile for vendor: {VendorId}", id);

        await _sender.Send(command, cancellationToken);

        return NoContent();
    }
}
