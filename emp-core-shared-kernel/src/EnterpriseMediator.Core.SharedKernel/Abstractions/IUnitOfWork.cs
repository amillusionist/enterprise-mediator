namespace EnterpriseMediator.Core.SharedKernel.Abstractions;

/// <summary>
/// Represents a unit of work for coordinating the persistence of one or more aggregate mutations
/// within a single database transaction. One SaveChangesAsync call per business operation.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Persists all pending changes tracked by the unit of work.
    /// </summary>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
