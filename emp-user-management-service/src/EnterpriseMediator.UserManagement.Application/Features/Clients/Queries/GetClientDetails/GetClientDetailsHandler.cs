using EnterpriseMediator.UserManagement.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EnterpriseMediator.UserManagement.Application.Features.Clients.Queries.GetClientDetails;

public class GetClientDetailsHandler : IRequestHandler<GetClientDetailsQuery, ClientDetailsDto>
{
    private readonly IClientRepository _clientRepository;
    private readonly ILogger<GetClientDetailsHandler> _logger;

    public GetClientDetailsHandler(IClientRepository clientRepository, ILogger<GetClientDetailsHandler> logger)
    {
        _clientRepository = clientRepository ?? throw new ArgumentNullException(nameof(clientRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ClientDetailsDto> Handle(GetClientDetailsQuery request, CancellationToken cancellationToken)
    {
        var client = await _clientRepository.GetByIdAsync(request.ClientId, cancellationToken);
        if (client is null)
        {
            throw new KeyNotFoundException($"Client with ID '{request.ClientId}' was not found.");
        }

        _logger.LogDebug("Retrieved client details for {ClientId}", request.ClientId);

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
