using EnterpriseMediator.UserManagement.Domain.Aggregates.User;
using EnterpriseMediator.UserManagement.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EnterpriseMediator.UserManagement.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly UserDbContext _dbContext;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(UserDbContext dbContext, ILogger<UserRepository> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        var normalizedEmail = email.ToLowerInvariant().Trim();
        return await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, ct);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var normalizedEmail = email.ToLowerInvariant().Trim();
        return await _dbContext.Users
            .AnyAsync(u => u.Email == normalizedEmail, ct);
    }

    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        await _dbContext.Users.AddAsync(user, ct);
        _logger.LogDebug("User {UserId} added to context", user.Id);
    }

    public void Update(User user)
    {
        _dbContext.Users.Update(user);
        _logger.LogDebug("User {UserId} marked for update", user.Id);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _dbContext.SaveChangesAsync(ct);
    }
}
