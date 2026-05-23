using System.Reflection;
using EnterpriseMediator.ProjectManagement.Domain.Aggregates.ProjectAggregate;
using EnterpriseMediator.ProjectManagement.Domain.Events;
using EnterpriseMediator.ProjectManagement.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseMediator.ProjectManagement.Infrastructure.Persistence;

public class ProjectDbContext : DbContext, IUnitOfWork
{
    private readonly IPublisher _publisher;

    public ProjectDbContext(DbContextOptions<ProjectDbContext> options, IPublisher publisher) : base(options)
    {
        _publisher = publisher;
    }

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Proposal> Proposals => Set<Proposal>();
    public DbSet<SowDocument> SowDocuments => Set<SowDocument>();
    public DbSet<Milestone> Milestones => Set<Milestone>();
    public DbSet<ProjectPayoutRule> PayoutRules => Set<ProjectPayoutRule>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasPostgresExtension("uuid-ossp");
        modelBuilder.HasPostgresExtension("vector");
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await DispatchDomainEventsAsync(cancellationToken);
        return await base.SaveChangesAsync(cancellationToken);
    }

    private async Task DispatchDomainEventsAsync(CancellationToken ct)
    {
        var projectEntries = ChangeTracker.Entries<Project>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = projectEntries
            .SelectMany(e => e.DomainEvents)
            .ToList();

        foreach (var entity in projectEntries)
            entity.ClearDomainEvents();

        foreach (var domainEvent in domainEvents)
            await _publisher.Publish(domainEvent, ct);
    }
}
