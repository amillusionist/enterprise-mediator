using System.Net.Mime;
using System.Security.Claims;
using Emp.ApiGateway.Application.Features.Users.Commands;
using Emp.ApiGateway.Application.Features.Users.Queries;
using Emp.ApiGateway.Application.Interfaces.Infrastructure;
using EnterpriseMediator.Contracts.DTOs.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Emp.ApiGateway.Web.Controllers;

/// <summary>
/// Public API Controller for User-related operations.
/// Handles current user context, permissions, profile retrieval, and user invitations.
/// </summary>
[ApiController]
[Route("api/v1/users")]
[Authorize]
[Produces(MediaTypeNames.Application.Json)]
public class UsersController : ControllerBase
{
    private readonly ISender _mediator;
    private readonly ILogger<UsersController> _logger;

    public UsersController(ISender mediator, ILogger<UsersController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves the profile information for the currently authenticated user.
    /// Combines JWT claims with extended profile data from the User Service.
    /// </summary>
    /// <response code="200">User details retrieved successfully.</response>
    /// <response code="401">User is not authenticated.</response>
    [HttpGet("me")]
    [ProducesResponseType(typeof(CurrentUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CurrentUserResponse>> GetCurrentUser(CancellationToken ct)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var emailClaim = User.FindFirst(ClaimTypes.Email)?.Value;
        var nameClaim = User.FindFirst(ClaimTypes.Name)?.Value ?? User.Identity?.Name;

        var roles = User.FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .Union(User.FindAll("cognito:groups").Select(c => c.Value))
            .Distinct()
            .ToList();

        if (string.IsNullOrEmpty(userIdClaim))
        {
            _logger.LogWarning("Authenticated user request missing NameIdentifier claim.");
            return Unauthorized("User identity could not be verified.");
        }

        _logger.LogDebug("Retrieving context for user: {UserId}", userIdClaim);

        UserProfileDto? extendedProfile = null;
        if (Guid.TryParse(userIdClaim, out var userId))
        {
            extendedProfile = await _mediator.Send(new GetUserProfileQuery(userId), ct);
        }

        var response = new CurrentUserResponse
        {
            Id = userIdClaim,
            Email = extendedProfile?.Email ?? emailClaim,
            Name = extendedProfile?.FullName ?? nameClaim,
            Roles = roles,
            IsAuthenticated = true,
            LastLoginAt = extendedProfile?.LastLoginAt
        };

        return Ok(response);
    }

    /// <summary>
    /// Checks if the current user has a specific permission.
    /// Useful for frontend feature toggling.
    /// </summary>
    /// <param name="permission">The permission code to check.</param>
    /// <response code="200">Permission check result.</response>
    [HttpGet("permissions/check")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public ActionResult<bool> CheckPermission([FromQuery] string permission)
    {
        if (string.IsNullOrWhiteSpace(permission))
        {
            return BadRequest("Permission string cannot be empty.");
        }

        var hasPermission = User.HasClaim(c => c.Type == "permissions" && c.Value == permission);
        return Ok(hasPermission);
    }

    /// <summary>
    /// Invites a new user to the platform by sending an invitation email.
    /// </summary>
    /// <param name="request">Invitation details.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Invitation sent successfully.</response>
    /// <response code="400">Invalid invitation data.</response>
    [HttpPost("invite")]
    [ProducesResponseType(typeof(UserInvitationResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserInvitationResultDto>> InviteUser(
        [FromBody] InviteUserRequest request,
        CancellationToken ct)
    {
        var invitedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(invitedBy) || !Guid.TryParse(invitedBy, out var invitedByGuid))
        {
            return Unauthorized("Cannot determine inviting user.");
        }

        _logger.LogInformation("User invitation requested for email: {Email}, role: {Role}", request.Email, request.Role);

        var command = new InviteUserCommand(request.Email, request.Role, invitedByGuid);
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }

    /// <summary>
    /// Validates an invitation token (public endpoint for activation page).
    /// </summary>
    [HttpGet("/api/v1/public/invitations/{token}/validate")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UserInvitationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserInvitationDto>> ValidateInvitation(
        [FromRoute] string token,
        [FromServices] IUserServiceClient userService,
        CancellationToken ct)
    {
        var invitation = await userService.ValidateInvitationAsync(token, ct);
        if (invitation == null)
        {
            return NotFound("Invitation not found or expired.");
        }

        return Ok(invitation);
    }

    /// <summary>
    /// Activates a user from an invitation token (public endpoint).
    /// </summary>
    [HttpPost("/api/v1/public/invitations/{token}/activate")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ActivateUser(
        [FromRoute] string token,
        [FromBody] ActivateUserRequest request,
        [FromServices] IUserServiceClient userService,
        CancellationToken ct)
    {
        _logger.LogInformation("User activation requested via invitation token");
        await userService.ActivateUserAsync(token, request, ct);
        return Ok(new { message = "User activated successfully." });
    }
}

public record InviteUserRequest(string Email, string Role);

public record CurrentUserResponse
{
    public required string Id { get; init; }
    public string? Email { get; init; }
    public string? Name { get; init; }
    public required IReadOnlyList<string> Roles { get; init; }
    public required bool IsAuthenticated { get; init; }
    public DateTimeOffset? LastLoginAt { get; init; }
}
