using EnterpriseMediator.UserManagement.Application.Features.Clients.Queries.GetClientDetails;
using MediatR;

namespace EnterpriseMediator.UserManagement.Application.Features.Internal.Queries.GetClientById;

public sealed record GetClientByIdQuery(Guid ClientId) : IRequest<ClientDetailsDto>;
