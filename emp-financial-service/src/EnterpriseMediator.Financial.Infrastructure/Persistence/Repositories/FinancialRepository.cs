using EnterpriseMediator.Financial.Domain.Entities;
using EnterpriseMediator.Financial.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseMediator.Financial.Infrastructure.Persistence.Repositories;

public class FinancialRepository : IFinancialRepository
{
    private readonly FinancialDbContext _context;

    public FinancialRepository(FinancialDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public IUnitOfWork UnitOfWork => _context;

    // Invoice Operations

    public async Task<Invoice?> GetInvoiceByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Invoices
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    public async Task<Invoice?> GetInvoiceByProjectIdAsync(Guid projectId, CancellationToken cancellationToken)
    {
        return await _context.Invoices
            .FirstOrDefaultAsync(i => i.ProjectId == projectId, cancellationToken);
    }

    public async Task<Invoice?> GetInvoiceByPaymentIntentIdAsync(string paymentIntentId, CancellationToken cancellationToken)
    {
        return await _context.Invoices
            .FirstOrDefaultAsync(i => i.StripePaymentIntentId == paymentIntentId, cancellationToken);
    }

    public async Task AddInvoiceAsync(Invoice invoice, CancellationToken cancellationToken)
    {
        await _context.Invoices.AddAsync(invoice, cancellationToken);
    }

    public Task UpdateInvoiceAsync(Invoice invoice, CancellationToken cancellationToken)
    {
        _context.Invoices.Update(invoice);
        return Task.CompletedTask;
    }

    // Payout Operations

    public async Task<Payout?> GetPayoutByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Payouts
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Payout>> GetPendingPayoutsAsync(CancellationToken cancellationToken)
    {
        return await _context.Payouts
            .Where(p => p.Status == PayoutStatus.PendingApproval)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddPayoutAsync(Payout payout, CancellationToken cancellationToken)
    {
        await _context.Payouts.AddAsync(payout, cancellationToken);
    }

    public Task UpdatePayoutAsync(Payout payout, CancellationToken cancellationToken)
    {
        _context.Payouts.Update(payout);
        return Task.CompletedTask;
    }

    // Transaction Operations

    public async Task AddTransactionAsync(Transaction transaction, CancellationToken cancellationToken)
    {
        await _context.Transactions.AddAsync(transaction, cancellationToken);
    }

    public async Task<IEnumerable<Transaction>> GetTransactionsByProjectIdAsync(Guid projectId, CancellationToken cancellationToken)
    {
        return await _context.Transactions
            .Where(t => t.ProjectId == projectId)
            .OrderByDescending(t => t.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsTransactionWithExternalIdAsync(string externalId, CancellationToken cancellationToken)
    {
        return await _context.Transactions
            .AnyAsync(t => t.ExternalReferenceId == externalId, cancellationToken);
    }
}
