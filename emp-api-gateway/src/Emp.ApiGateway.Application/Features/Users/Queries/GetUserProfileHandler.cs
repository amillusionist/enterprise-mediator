using Emp.ApiGateway.Application.Interfaces.Infrastructure;
using EnterpriseMediator.Contracts.DTOs.Users;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Emp.ApiGateway.Application.Features.Users.Queries;

public class GetUserProfileHandler : IRequestHandler<GetUserProfileQuery, UserProfileDto?>
{
    private readonly IUserServiceClient _userService;
    private readonly ILogger<GetUserProfileHandler> _logger;

    public GetUserProfileHandler(IUserServiceClient userService, ILogger<GetUserProfileHandler> logger)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<UserProfileDto?> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching user profile for UserId: {UserId}", request.UserId);
        return await _userService.GetUserProfileAsync(request.UserId, cancellationToken);
    }
}
