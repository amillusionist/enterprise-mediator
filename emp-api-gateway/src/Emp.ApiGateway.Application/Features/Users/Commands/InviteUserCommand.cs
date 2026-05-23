using Emp.ApiGateway.Application.Interfaces.Infrastructure;
using MediatR;

namespace Emp.ApiGateway.Application.Features.Users.Commands;

public record InviteUserCommand(string Email, string Role, Guid InvitedBy) : IRequest<UserInvitationResultDto>;
