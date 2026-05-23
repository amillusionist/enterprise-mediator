using EnterpriseMediator.Financial.Domain.Entities;
using EnterpriseMediator.Financial.Domain.Enums;
using EnterpriseMediator.Financial.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseMediator.Financial.Infrastructure.Persistence.Repositories;

public class TransactionQueryRepository : ITransactionQueryRepository
{
    private readonly FinancialDbContext _context;

    public TransactionQueryRepository(FinancialDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<(IReadOnlyList<Transaction> Items, int TotalCount)> GetPagedTransactionsAsync(
        DateTime? startDate,
        DateTime? endDate,
        TransactionType? type,
        Guid? projectId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = _context.Transactions.AsNoTracking().AsQueryable();

        if (startDate.HasValue)
            query = query.Where(t => t.Timestamp >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(t => t.Timestamp <= endDate.Value);

        if (type.HasValue)
            query = query.Where(t => t.Type == type.Value);

        if (projectId.HasValue)
            query = query.Where(t => t.ProjectId == projectId.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(t => t.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
