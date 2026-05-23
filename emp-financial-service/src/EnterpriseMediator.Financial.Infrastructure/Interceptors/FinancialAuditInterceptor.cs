using EnterpriseMediator.Financial.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EnterpriseMediator.Financial.Infrastructure.Interceptors
{
    public class FinancialAuditInterceptor : SaveChangesInterceptor
    {
        private readonly IDateTime _dateTimeProvider;

        public FinancialAuditInterceptor(IDateTime dateTimeProvider)
        {
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        }

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            UpdateAuditFields(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            UpdateAuditFields(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void UpdateAuditFields(DbContext? context)
        {
            if (context == null) return;

            var utcNow = _dateTimeProvider.UtcNow;

            foreach (var entry in context.ChangeTracker.Entries())
            {
                if (entry.State == EntityState.Added)
                {
                    SetPropertyIfPresent(entry, "CreatedOn", utcNow);
                    SetPropertyIfPresent(entry, "LastModifiedOn", utcNow);
                }
                else if (entry.State == EntityState.Modified || entry.HasChangedOwnedEntities())
                {
                    SetPropertyIfPresent(entry, "LastModifiedOn", utcNow);
                }
            }
        }

        private void SetPropertyIfPresent(EntityEntry entry, string propertyName, DateTime value)
        {
            var property = entry.Metadata.FindProperty(propertyName);
            if (property != null && property.ClrType == typeof(DateTime))
            {
                entry.Property(propertyName).CurrentValue = value;
            }
            else if (property != null && property.ClrType == typeof(DateTime?))
            {
                entry.Property(propertyName).CurrentValue = value;
            }
            else if (property != null && property.ClrType == typeof(DateTimeOffset))
            {
                entry.Property(propertyName).CurrentValue = (DateTimeOffset)value;
            }
            else if (property != null && property.ClrType == typeof(DateTimeOffset?))
            {
                entry.Property(propertyName).CurrentValue = (DateTimeOffset)value;
            }
        }
    }

    public static class EntityEntryExtensions
    {
        public static bool HasChangedOwnedEntities(this EntityEntry entry) =>
            entry.References.Any(r =>
                r.TargetEntry != null &&
                r.TargetEntry.Metadata.IsOwned() &&
                (r.TargetEntry.State == EntityState.Added || r.TargetEntry.State == EntityState.Modified));
    }
}
