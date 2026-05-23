using EnterpriseMediator.UserManagement.Application.Features.Clients.Queries.GetClientDetails;
using EnterpriseMediator.UserManagement.Domain.Interfaces;
using MediatR;

namespace EnterpriseMediator.UserManagement.Application.Features.Internal.Queries.GetClientById;

public class GetClientByIdHandler : IRequestHandler<GetClientByIdQuery, ClientDetailsDto>
{
    private readonly IClientRepository _clientRepository;

    public GetClientByIdHandler(IClientRepository clientRepository)
    {
        _clientRepository = clientRepository ?? throw new ArgumentNullException(nameof(clientRepository));
    }

    public async Task<ClientDetailsDto> Handle(GetClientByIdQuery request, CancellationToken cancellationToken)
    {
        var client = await _clientRepository.GetByIdAsync(request.ClientId, cancellationToken);
        if (client is null)
        {
            throw new KeyNotFoundException($"Client with ID '{request.ClientId}' was not found.");
        }

        return new ClientDetailsDto
        {
            Id = client.Id,
            CompanyName = client.CompanyName,
            Status = client.Status,
            CreatedAt = client.CreatedAt.DateTime,
            Address = new ClientAddressDto
            {
                AddressLine1 = client.CompanyAddress?.Street ?? string.Empty,
                City = client.CompanyAddress?.City ?? string.Empty,
                State = client.CompanyAddress?.State ?? string.Empty,
                PostalCode = client.CompanyAddress?.PostalCode ?? string.Empty,
                Country = client.CompanyAddress?.Country ?? string.Empty
            }
        };
    }
}
