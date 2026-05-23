namespace EnterpriseMediator.Core.SharedKernel.Abstractions;

/// <summary>
/// Defines the contract for a read-only repository.
/// </summary>
/// <typeparam name="T">The type of the entity.</typeparam>
public interface IReadRepository<T> where T : class
{
    /// <summary>
    /// Retrieves an entity by its untyped identifier.
    /// </summary>
    Task<T?> GetByIdAsync(object id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves an entity by a strongly-typed identifier.
    /// </summary>
    Task<T?> GetByIdAsync<TId>(TId id, CancellationToken cancellationToken = default) where TId : notnull;

    /// <summary>
    /// Retrieves a list of all entities.
    /// </summary>
    Task<List<T>> ListAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a list of entities matching the specified specification.
    /// </summary>
    Task<List<T>> ListAsync(ISpecification<T> spec, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the first entity matching the specification, or null.
    /// </summary>
    Task<T?> FirstOrDefaultAsync(ISpecification<T> spec, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the single entity matching the specification, or null.
    /// Throws if more than one entity matches.
    /// </summary>
    Task<T?> SingleOrDefaultAsync(ISpecification<T> spec, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the number of entities matching the specified specification.
    /// </summary>
    Task<int> CountAsync(ISpecification<T> spec, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if any entity exists.
    /// </summary>
    Task<bool> AnyAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if any entity matches the specified specification.
    /// </summary>
    Task<bool> AnyAsync(ISpecification<T> spec, CancellationToken cancellationToken = default);
}