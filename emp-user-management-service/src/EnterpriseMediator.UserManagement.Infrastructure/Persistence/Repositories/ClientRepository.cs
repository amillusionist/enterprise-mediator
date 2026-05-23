using EnterpriseMediator.UserManagement.Domain.Aggregates.Client;
using EnterpriseMediator.UserManagement.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EnterpriseMediator.UserManagement.Infrastructure.Persistence.Repositories;

public class ClientRepository : IClientRepository
{
    private readonly UserDbContext _dbContext;
    private readonly ILogger<ClientRepository> _logger;

    public ClientRepository(UserDbContext dbContext, ILogger<ClientRepository> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Client?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbContext.Clients
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<Client?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        var normalizedEmail = email.ToLowerInvariant().Trim();
        return await _dbContext.Clients
            .FirstOrDefaultAsync(c => c.PrimaryContactEmail == normalizedEmail, ct);
    }

    public async Task AddAsync(Client client, CancellationToken ct = default)
    {
        await _dbContext.Clients.AddAsync(client, ct);
        _logger.LogDebug("Client {ClientId} added to context", client.Id);
    }

    public void Update(Client client)
    {
        _dbContext.Clients.Update(client);
        _logger.LogDebug("Client {ClientId} marked for update", client.Id);
    }

    public async Task<IEnumerable<Client>> ListAsync(int page, int pageSize, CancellationToken ct = default)
    {
        return await _dbContext.Clients
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _dbContext.SaveChangesAsync(ct);
    }
}
