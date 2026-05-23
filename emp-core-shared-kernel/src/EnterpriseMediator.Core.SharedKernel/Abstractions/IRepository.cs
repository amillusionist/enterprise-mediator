namespace EnterpriseMediator.Core.SharedKernel.Abstractions;

/// <summary>
/// Defines the contract for a repository that handles read and write operations for an aggregate root.
/// Mutations track changes only; call <see cref="IUnitOfWork.SaveChangesAsync"/> to persist.
/// </summary>
/// <typeparam name="T">The type of the aggregate root entity.</typeparam>
public interface IRepository<T> : IReadRepository<T> where T : class
{
    /// <summary>
    /// Gets the unit of work associated with this repository, for explicit persistence control.
    /// </summary>
    IUnitOfWork UnitOfWork { get; }

    /// <summary>
    /// Adds a new entity to the change tracker.
    /// </summary>
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a collection of entities to the change tracker.
    /// </summary>
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an entity as modified in the change tracker.
    /// </summary>
    void Update(T entity);

    /// <summary>
    /// Marks a collection of entities as modified in the change tracker.
    /// </summary>
    void UpdateRange(IEnumerable<T> entities);

    /// <summary>
    /// Marks an entity for deletion in the change tracker.
    /// </summary>
    void Delete(T entity);

    /// <summary>
    /// Marks a collection of entities for deletion in the change tracker.
    /// </summary>
    void DeleteRange(IEnumerable<T> entities);
}