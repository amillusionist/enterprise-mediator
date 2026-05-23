using System.Net.Mime;
using Emp.ApiGateway.Application.Interfaces.Infrastructure;
using EnterpriseMediator.Contracts.Common;
using EnterpriseMediator.Contracts.DTOs.Vendors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Emp.ApiGateway.Web.Controllers;

/// <summary>
/// Manages vendor profiles, activation, and contact invitations.
/// Delegates to the User Management Microservice.
/// </summary>
[ApiController]
[Route("api/v1/vendors")]
[Authorize]
[Produces(MediaTypeNames.Application.Json)]
public class VendorsController : ControllerBase
{
    private readonly IUserServiceClient _userService;
    private readonly ILogger<VendorsController> _logger;

    public VendorsController(IUserServiceClient userService, ILogger<VendorsController> logger)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves a paginated list of vendors with optional filters.
    /// </summary>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="search">Optional search term for company name.</param>
    /// <param name="status">Optional vendor status filter.</param>
    /// <param name="skills">Optional comma-separated skills filter.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Paginated list of vendors.</returns>
    /// <response code="200">Vendors retrieved successfully.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<VendorDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResultDto<VendorDto>>> GetVendors(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        [FromQuery] string? skills = null,
        CancellationToken ct = default)
    {
        var result = await _userService.GetVendorsAsync(page, pageSize, search, status, skills, ct);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves detailed vendor profile by ID.
    /// </summary>
    /// <param name="vendorId">The vendor identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Vendor detail information.</returns>
    /// <response code="200">Vendor retrieved successfully.</response>
    /// <response code="404">Vendor not found.</response>
    [HttpGet("{vendorId:guid}")]
    [ProducesResponseType(typeof(VendorDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VendorDetailDto>> GetVendorById(
        [FromRoute] Guid vendorId,
        CancellationToken ct)
    {
        var result = await _userService.GetVendorByIdAsync(vendorId, ct);

        if (result == null)
        {
            _logger.LogWarning("Vendor not found for ID: {VendorId}", vendorId);
            return NotFound($"Vendor with ID {vendorId} not found.");
        }

        return Ok(result);
    }

    /// <summary>
    /// Creates a new vendor profile.
    /// </summary>
    /// <param name="request">Vendor creation details.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The created vendor profile.</returns>
    /// <response code="201">Vendor created successfully.</response>
    /// <response code="400">Invalid vendor data.</response>
    [HttpPost]
    [ProducesResponseType(typeof(VendorDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<VendorDto>> CreateVendor(
        [FromBody] CreateVendorRequest request,
        CancellationToken ct)
    {
        _logger.LogInformation("Creating vendor: {CompanyName}", request.CompanyName);
        var vendor = await _userService.CreateVendorAsync(request, ct);
        return CreatedAtAction(nameof(GetVendorById), new { vendorId = vendor.Id }, vendor);
    }

    /// <summary>
    /// Updates an existing vendor profile.
    /// </summary>
    /// <param name="vendorId">The vendor identifier.</param>
    /// <param name="request">Fields to update.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The updated vendor profile.</returns>
    /// <response code="200">Vendor updated successfully.</response>
    /// <response code="404">Vendor not found.</response>
    [HttpPatch("{vendorId:guid}")]
    [ProducesResponseType(typeof(VendorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VendorDto>> UpdateVendor(
        [FromRoute] Guid vendorId,
        [FromBody] UpdateVendorRequest request,
        CancellationToken ct)
    {
        _logger.LogInformation("Updating vendor: {VendorId}", vendorId);
        var vendor = await _userService.UpdateVendorAsync(vendorId, request, ct);
        return Ok(vendor);
    }

    /// <summary>
    /// Activates a vendor, allowing them to receive project briefs.
    /// </summary>
    /// <param name="vendorId">The vendor identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Vendor activated.</response>
    [HttpPost("{vendorId:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ActivateVendor(
        [FromRoute] Guid vendorId,
        CancellationToken ct)
    {
        _logger.LogInformation("Activating vendor: {VendorId}", vendorId);
        await _userService.ActivateVendorAsync(vendorId, ct);
        return Ok(new { message = "Vendor activated." });
    }

    /// <summary>
    /// Deactivates a vendor, preventing them from receiving project briefs.
    /// </summary>
    /// <param name="vendorId">The vendor identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Vendor deactivated.</response>
    [HttpPost("{vendorId:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DeactivateVendor(
        [FromRoute] Guid vendorId,
        CancellationToken ct)
    {
        _logger.LogInformation("Deactivating vendor: {VendorId}", vendorId);
        await _userService.DeactivateVendorAsync(vendorId, ct);
        return Ok(new { message = "Vendor deactivated." });
    }

    /// <summary>
    /// Sends an invitation email to a vendor contact.
    /// </summary>
    /// <param name="vendorId">The vendor identifier.</param>
    /// <param name="request">The contact email to invite.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Invitation sent.</response>
    [HttpPost("{vendorId:guid}/contacts/invite")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> InviteContact(
        [FromRoute] Guid vendorId,
        [FromBody] InviteContactRequest request,
        CancellationToken ct)
    {
        _logger.LogInformation("Inviting contact for vendor {VendorId}: {Email}", vendorId, request.Email);
        await _userService.InviteVendorContactAsync(vendorId, request.Email, ct);
        return Ok(new { message = "Invitation sent." });
    }
}

/// <summary>
/// Request payload for inviting a vendor contact.
/// </summary>
public record InviteContactRequest(string Email);
