using System.Net.Mime;
using Emp.ApiGateway.Application.Interfaces.Infrastructure;
using EnterpriseMediator.Contracts.Common;
using EnterpriseMediator.Contracts.DTOs.Clients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Emp.ApiGateway.Web.Controllers;

/// <summary>
/// Manages client company profiles.
/// Delegates to the User Management Microservice.
/// </summary>
[ApiController]
[Route("api/v1/clients")]
[Authorize]
[Produces(MediaTypeNames.Application.Json)]
public class ClientsController : ControllerBase
{
    private readonly IUserServiceClient _userService;
    private readonly ILogger<ClientsController> _logger;

    public ClientsController(IUserServiceClient userService, ILogger<ClientsController> logger)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves a paginated list of clients with optional filters.
    /// </summary>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="search">Optional search term for company name.</param>
    /// <param name="status">Optional client status filter.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Paginated list of clients.</returns>
    /// <response code="200">Clients retrieved successfully.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<ClientDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResultDto<ClientDto>>> GetClients(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        CancellationToken ct = default)
    {
        var result = await _userService.GetClientsAsync(page, pageSize, search, status, ct);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a client profile by ID.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Client profile details.</returns>
    /// <response code="200">Client retrieved successfully.</response>
    /// <response code="404">Client not found.</response>
    [HttpGet("{clientId:guid}")]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientDto>> GetClientById(
        [FromRoute] Guid clientId,
        CancellationToken ct)
    {
        var result = await _userService.GetClientByIdAsync(clientId, ct);

        if (result == null)
        {
            _logger.LogWarning("Client not found for ID: {ClientId}", clientId);
            return NotFound($"Client with ID {clientId} not found.");
        }

        return Ok(result);
    }

    /// <summary>
    /// Creates a new client profile.
    /// </summary>
    /// <param name="request">Client creation details.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The created client profile.</returns>
    /// <response code="201">Client created successfully.</response>
    /// <response code="400">Invalid client data.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ClientDto>> CreateClient(
        [FromBody] CreateClientRequest request,
        CancellationToken ct)
    {
        _logger.LogInformation("Creating client: {CompanyName}", request.CompanyName);
        var client = await _userService.CreateClientAsync(request, ct);
        return CreatedAtAction(nameof(GetClientById), new { clientId = client.Id }, client);
    }

    /// <summary>
    /// Updates an existing client profile.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="request">Updated client data.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The updated client profile.</returns>
    /// <response code="200">Client updated successfully.</response>
    /// <response code="404">Client not found.</response>
    [HttpPatch("{clientId:guid}")]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientDto>> UpdateClient(
        [FromRoute] Guid clientId,
        [FromBody] CreateClientRequest request,
        CancellationToken ct)
    {
        _logger.LogInformation("Updating client: {ClientId}", clientId);
        var client = await _userService.UpdateClientAsync(clientId, request, ct);
        return Ok(client);
    }

    /// <summary>
    /// Deactivates a client, preventing new project creation.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Client deactivated.</response>
    [HttpPut("{clientId:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DeactivateClient(
        [FromRoute] Guid clientId,
        CancellationToken ct)
    {
        _logger.LogInformation("Deactivating client: {ClientId}", clientId);
        await _userService.DeactivateClientAsync(clientId, ct);
        return Ok(new { message = "Client deactivated." });
    }

    /// <summary>
    /// Reactivates a previously deactivated client.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Client reactivated.</response>
    [HttpPut("{clientId:guid}/reactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ReactivateClient(
        [FromRoute] Guid clientId,
        CancellationToken ct)
    {
        _logger.LogInformation("Reactivating client: {ClientId}", clientId);
        await _userService.ReactivateClientAsync(clientId, ct);
        return Ok(new { message = "Client reactivated." });
    }
}
