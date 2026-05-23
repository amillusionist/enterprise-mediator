using EnterpriseMediator.UserManagement.Application.Features.Users.Commands.AnonymizeUser;
using EnterpriseMediator.UserManagement.Application.Features.Users.Commands.RegisterUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseMediator.UserManagement.API.Controllers;

/// <summary>
/// Manages user operations including registration and GDPR compliance.
/// </summary>
[ApiController]
[Route("api/v1/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ILogger<UsersController> _logger;

    public UsersController(ISender sender, ILogger<UsersController> logger)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Registers a new user in the system.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "SystemAdmin")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Guid>> RegisterUser(
        [FromBody] RegisterUserCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Registering new user with type {UserType}", command.UserType);

        var userId = await _sender.Send(command, cancellationToken);

        return Created(new Uri($"/api/v1/users/{userId}", UriKind.Relative), userId);
    }

    /// <summary>
    /// Anonymizes a user's PII for GDPR compliance (Right to be Forgotten).
    /// </summary>
    [HttpPost("{id}/anonymize")]
    [Authorize(Roles = "SystemAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AnonymizeUser(Guid id, [FromBody] AnonymizeUserRequest request, CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
            return BadRequest("Invalid User ID.");

        await _sender.Send(new AnonymizeUserCommand(id, request.Reason), cancellationToken);

        _logger.LogInformation("User {UserId} anonymized", id);

        return NoContent();
    }
}

public record AnonymizeUserRequest
{
    public string Reason { get; init; } = string.Empty;
}
