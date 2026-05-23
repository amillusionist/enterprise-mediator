using EnterpriseMediator.UserManagement.Application.Interfaces;
using EnterpriseMediator.UserManagement.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EnterpriseMediator.UserManagement.Application.Features.Users.Commands.AnonymizeUser;

public class AnonymizeUserHandler : IRequestHandler<AnonymizeUserCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly IAuditServiceAdapter _auditAdapter;
    private readonly ILogger<AnonymizeUserHandler> _logger;

    public AnonymizeUserHandler(
        IUserRepository userRepository,
        IAuditServiceAdapter auditAdapter,
        ILogger<AnonymizeUserHandler> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _auditAdapter = auditAdapter ?? throw new ArgumentNullException(nameof(auditAdapter));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> Handle(AnonymizeUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            throw new KeyNotFoundException($"User with ID '{request.UserId}' was not found.");
        }

        user.Anonymize();
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(cancellationToken);

        await _auditAdapter.LogEventAsync(new AuditLogEntry(
            request.UserId,
            "UserAnonymized",
            "User",
            request.UserId.ToString(),
            $"GDPR anonymization. Reason: {request.Reason}"),
            cancellationToken);

        _logger.LogInformation("User {UserId} anonymized for reason: {Reason}", request.UserId, request.Reason);

        return true;
    }
}
