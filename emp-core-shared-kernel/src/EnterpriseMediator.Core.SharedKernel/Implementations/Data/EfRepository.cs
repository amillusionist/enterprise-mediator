using EnterpriseMediator.Core.SharedKernel.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseMediator.Core.SharedKernel.Implementations.Data;

/// <summary>
/// A generic repository implementation using Entity Framework Core.
/// Mutations track changes only — call <see cref="IUnitOfWork.SaveChangesAsync"/> to persist.
/// </summary>
/// <typeparam name="T">The entity type this repository works with.</typeparam>
public class EfRepository<T> : IRepository<T> where T : class
{
    protected readonly DbContext _dbContext;
    protected readonly DbSet<T> _dbSet;

    public EfRepository(DbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _dbSet = _dbContext.Set<T>();
    }

    /// <inheritdoc />
    public IUnitOfWork UnitOfWork => (IUnitOfWork)_dbContext;

    // ── Reads ──────────────────────────────────────────────────

    /// <inheritdoc />
    public virtual async Task<T?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new[] { id }, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<T?> GetByIdAsync<TId>(TId id, CancellationToken cancellationToken = default) where TId : notnull
    {
        return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<List<T>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<List<T>> ListAsync(ISpecification<T> spec, CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(spec).ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<T?> FirstOrDefaultAsync(ISpecification<T> spec, CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(spec).FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<T?> SingleOrDefaultAsync(ISpecification<T> spec, CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(spec).SingleOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<int> CountAsync(ISpecification<T> spec, CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(spec).CountAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<bool> AnyAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<bool> AnyAsync(ISpecification<T> spec, CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(spec).AnyAsync(cancellationToken);
    }

    // ── Writes (track only, no auto-save) ──────────────────────

    /// <inheritdoc />
    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await _dbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    /// <inheritdoc />
    public virtual async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities);
        await _dbSet.AddRangeAsync(entities, cancellationToken);
    }

    /// <inheritdoc />
    public virtual void Update(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _dbSet.Update(entity);
    }

    /// <inheritdoc />
    public virtual void UpdateRange(IEnumerable<T> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);
        _dbSet.UpdateRange(entities);
    }

    /// <inheritdoc />
    public virtual void Delete(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _dbSet.Remove(entity);
    }

    /// <inheritdoc />
    public virtual void DeleteRange(IEnumerable<T> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);
        _dbSet.RemoveRange(entities);
    }

    // ── Specification ──────────────────────────────────────────

    protected virtual IQueryable<T> ApplySpecification(ISpecification<T> spec)
    {
        return SpecificationEvaluator<T>.GetQuery(_dbSet.AsQueryable(), spec);
    }
}