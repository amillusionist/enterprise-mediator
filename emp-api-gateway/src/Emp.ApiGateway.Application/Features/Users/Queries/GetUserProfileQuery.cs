using EnterpriseMediator.Contracts.DTOs.Users;
using MediatR;

namespace Emp.ApiGateway.Application.Features.Users.Queries;

public record GetUserProfileQuery(Guid UserId) : IRequest<UserProfileDto?>;
