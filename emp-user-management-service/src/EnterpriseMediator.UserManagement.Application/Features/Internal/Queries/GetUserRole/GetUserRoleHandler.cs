using EnterpriseMediator.UserManagement.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EnterpriseMediator.UserManagement.Application.Features.Internal.Queries.GetUserRole;

public class GetUserRoleHandler : IRequestHandler<GetUserRoleQuery, string>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<GetUserRoleHandler> _logger;

    public GetUserRoleHandler(IUserRepository userRepository, ILogger<GetUserRoleHandler> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> Handle(GetUserRoleQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            throw new KeyNotFoundException($"User with ID '{request.UserId}' was not found.");
        }

        _logger.LogDebug("Retrieved role for user {UserId}: {UserType}", request.UserId, user.Type);

        return user.Type.ToString();
    }
}
