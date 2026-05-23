using MediatR;

namespace EnterpriseMediator.UserManagement.Application.Features.Users.Commands.RegisterUser;

public sealed record RegisterUserCommand : IRequest<Guid>
{
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string UserType { get; init; } = string.Empty;
    public Guid? ProfileId { get; init; }
}
