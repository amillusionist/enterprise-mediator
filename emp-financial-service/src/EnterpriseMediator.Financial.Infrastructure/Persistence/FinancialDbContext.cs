using System.Reflection;
using EnterpriseMediator.Financial.Domain.Entities;
using EnterpriseMediator.Financial.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseMediator.Financial.Infrastructure.Persistence
{
    /// <summary>
    /// The Entity Framework Core Database Context for the Financial Microservice.
    /// Manages persistence for Invoices, Payouts, and Transactions.
    /// Implements IUnitOfWork to support the repository pattern.
    /// </summary>
    public class FinancialDbContext : DbContext, IUnitOfWork
    {
        public FinancialDbContext(DbContextOptions<FinancialDbContext> options)
            : base(options)
        {
        }

        public DbSet<Invoice> Invoices => Set<Invoice>();
        public DbSet<Payout> Payouts => Set<Payout>();
        public DbSet<Transaction> Transactions => Set<Transaction>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("financial");
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(modelBuilder);
        }
    }
}
