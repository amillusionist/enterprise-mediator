using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EnterpriseMediator.Financial.Domain.Entities;

namespace EnterpriseMediator.Financial.Domain.Interfaces
{
    /// <summary>
    /// Defines the Unit of Work contract for managing transactions across the financial domain.
    /// </summary>
    public interface IUnitOfWork
    {
        /// <summary>
        /// Saves all changes made in this context to the database.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The number of state entries written to the database.</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Defines the contract for persisting and retrieving financial aggregate roots.
    /// This repository pattern abstracts the underlying data access technology (EF Core).
    /// </summary>
    public interface IFinancialRepository
    {
        /// <summary>
        /// Gets the Unit of Work associated with this repository.
        /// </summary>
        IUnitOfWork UnitOfWork { get; }

        // Invoice Operations

        /// <summary>Retrieves an invoice by its unique identifier.</summary>
        Task<Invoice?> GetInvoiceByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>Retrieves an invoice by its associated project identifier.</summary>
        Task<Invoice?> GetInvoiceByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);

        /// <summary>Retrieves an invoice by the Stripe payment intent identifier.</summary>
        Task<Invoice?> GetInvoiceByPaymentIntentIdAsync(string paymentIntentId, CancellationToken cancellationToken = default);

        /// <summary>Adds a new invoice to the repository.</summary>
        Task AddInvoiceAsync(Invoice invoice, CancellationToken cancellationToken = default);

        /// <summary>Updates an existing invoice in the repository.</summary>
        Task UpdateInvoiceAsync(Invoice invoice, CancellationToken cancellationToken = default);

        // Payout Operations

        /// <summary>Retrieves a payout by its unique identifier.</summary>
        Task<Payout?> GetPayoutByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>Retrieves all payouts in PendingApproval status.</summary>
        Task<IEnumerable<Payout>> GetPendingPayoutsAsync(CancellationToken cancellationToken = default);

        /// <summary>Adds a new payout to the repository.</summary>
        Task AddPayoutAsync(Payout payout, CancellationToken cancellationToken = default);

        /// <summary>Updates an existing payout in the repository.</summary>
        Task UpdatePayoutAsync(Payout payout, CancellationToken cancellationToken = default);

        // Transaction/Ledger Operations

        /// <summary>Adds an immutable transaction record to the ledger.</summary>
        Task AddTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default);

        /// <summary>Retrieves all transactions for a given project.</summary>
        Task<IEnumerable<Transaction>> GetTransactionsByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Checks if a transaction with the given external reference ID already exists.
        /// Critical for idempotency checks on webhooks.
        /// </summary>
        Task<bool> ExistsTransactionWithExternalIdAsync(string externalId, CancellationToken cancellationToken = default);
    }
}