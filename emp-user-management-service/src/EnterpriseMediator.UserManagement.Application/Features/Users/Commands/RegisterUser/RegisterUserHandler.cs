using EnterpriseMediator.UserManagement.Application.Configuration;
using EnterpriseMediator.UserManagement.Application.Interfaces;
using EnterpriseMediator.UserManagement.Domain.Aggregates.User;
using EnterpriseMediator.UserManagement.Domain.Enums;
using EnterpriseMediator.UserManagement.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EnterpriseMediator.UserManagement.Application.Features.Users.Commands.RegisterUser;

public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, Guid>
{
    private readonly IUserRepository _userRepository;
    private readonly IIdentityService _identityService;
    private readonly UserManagementSettings _settings;
    private readonly ILogger<RegisterUserHandler> _logger;

    public RegisterUserHandler(
        IUserRepository userRepository,
        IIdentityService identityService,
        IOptions<UserManagementSettings> settings,
        ILogger<RegisterUserHandler> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Guid> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var emailExists = await _userRepository.ExistsByEmailAsync(request.Email, cancellationToken);
        if (emailExists)
        {
            throw new InvalidOperationException($"A user with email '{request.Email}' already exists.");
        }

        var userType = Enum.Parse<UserType>(request.UserType);
        var defaultRole = userType switch
        {
            UserType.Internal => _settings.DefaultInternalRole,
            UserType.Client => _settings.DefaultClientRole,
            UserType.Vendor => _settings.DefaultVendorRole,
            _ => throw new ArgumentOutOfRangeException(nameof(request), request.UserType, "Unsupported user type")
        };

        var cognitoUserId = await _identityService.CreateUserAsync(
            request.Email, request.Password, defaultRole, cancellationToken);

        var user = User.Create(
            request.Email,
            request.FirstName,
            request.LastName,
            cognitoUserId,
            userType,
            request.ProfileId);

        await _userRepository.AddAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} registered with type {UserType}", user.Id, userType);

        return user.Id;
    }
}
