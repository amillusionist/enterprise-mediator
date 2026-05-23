using EnterpriseMediator.Financial.Domain.Entities;
using EnterpriseMediator.Financial.Domain.Enums;

namespace EnterpriseMediator.Financial.Domain.Interfaces;

/// <summary>
/// Read-only repository for querying transactions with filtering and pagination.
/// </summary>
public interface ITransactionQueryRepository
{
    /// <summary>
    /// Retrieves a paged list of transactions with optional filters.
    /// </summary>
    /// <param name="startDate">Optional start date filter (inclusive).</param>
    /// <param name="endDate">Optional end date filter (inclusive).</param>
    /// <param name="type">Optional transaction type filter.</param>
    /// <param name="projectId">Optional project ID filter.</param>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A tuple containing the paged items and total count.</returns>
    Task<(IReadOnlyList<Transaction> Items, int TotalCount)> GetPagedTransactionsAsync(
        DateTime? startDate,
        DateTime? endDate,
        TransactionType? type,
        Guid? projectId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
