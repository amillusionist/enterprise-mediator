using Emp.ApiGateway.Application.Interfaces.Infrastructure;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Emp.ApiGateway.Application.Features.Users.Commands;

public class InviteUserHandler : IRequestHandler<InviteUserCommand, UserInvitationResultDto>
{
    private readonly IUserServiceClient _userService;
    private readonly ILogger<InviteUserHandler> _logger;

    public InviteUserHandler(IUserServiceClient userService, ILogger<InviteUserHandler> logger)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<UserInvitationResultDto> Handle(InviteUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Inviting user: {Email} with role: {Role}", request.Email, request.Role);
        var dto = new InviteUserDto(request.Email, request.Role, request.InvitedBy);
        return await _userService.InviteUserAsync(dto, cancellationToken);
    }
}
