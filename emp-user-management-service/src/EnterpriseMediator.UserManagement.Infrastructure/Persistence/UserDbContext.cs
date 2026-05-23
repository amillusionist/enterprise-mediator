using System.Reflection;
using EnterpriseMediator.UserManagement.Domain.Aggregates.Client;
using EnterpriseMediator.UserManagement.Domain.Aggregates.User;
using EnterpriseMediator.UserManagement.Domain.Aggregates.Vendor;
using EnterpriseMediator.UserManagement.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseMediator.UserManagement.Infrastructure.Persistence;

public class UserDbContext : DbContext
{
    private readonly DomainEventDispatcher? _domainEventDispatcher;

    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
    {
    }

    public UserDbContext(DbContextOptions<UserDbContext> options, DomainEventDispatcher domainEventDispatcher)
        : base(options)
    {
        _domainEventDispatcher = domainEventDispatcher;
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<Client> Clients => Set<Client>();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (_domainEventDispatcher is not null)
        {
            await _domainEventDispatcher.DispatchEventsAsync(this, cancellationToken);
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                {
                    property.SetValueConverter(
                        new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
                            v => v.ToUniversalTime(),
                            v => DateTime.SpecifyKind(v, DateTimeKind.Utc)));
                }
            }
        }
    }
}
