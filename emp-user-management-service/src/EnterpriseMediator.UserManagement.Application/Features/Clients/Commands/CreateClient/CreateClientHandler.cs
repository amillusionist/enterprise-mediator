using EnterpriseMediator.UserManagement.Domain.Aggregates.Client;
using EnterpriseMediator.UserManagement.Domain.Interfaces;
using EnterpriseMediator.UserManagement.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EnterpriseMediator.UserManagement.Application.Features.Clients.Commands.CreateClient;

public class CreateClientHandler : IRequestHandler<CreateClientCommand, Guid>
{
    private readonly IClientRepository _clientRepository;
    private readonly ILogger<CreateClientHandler> _logger;

    public CreateClientHandler(IClientRepository clientRepository, ILogger<CreateClientHandler> logger)
    {
        _clientRepository = clientRepository ?? throw new ArgumentNullException(nameof(clientRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Guid> Handle(CreateClientCommand request, CancellationToken cancellationToken)
    {
        var existingClient = await _clientRepository.GetByEmailAsync(request.PrimaryContactEmail, cancellationToken);
        if (existingClient is not null)
        {
            throw new InvalidOperationException($"A client with email '{request.PrimaryContactEmail}' already exists.");
        }

        var companyAddress = Address.Create(
            request.AddressLine1,
            request.City,
            request.State,
            request.PostalCode,
            request.Country);

        var billingAddress = Address.Create(
            request.AddressLine1,
            request.City,
            request.State,
            request.PostalCode,
            request.Country);

        var client = Client.Create(
            request.CompanyName,
            companyAddress,
            billingAddress,
            request.PrimaryContactEmail);

        await _clientRepository.AddAsync(client, cancellationToken);
        await _clientRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Client {ClientId} created: {CompanyName}", client.Id, client.CompanyName);

        return client.Id;
    }
}
