using System.Linq.Expressions;
using EnterpriseMediator.Core.SharedKernel.Abstractions;

namespace EnterpriseMediator.Core.SharedKernel.Common;

/// <summary>
/// Base implementation of the Specification pattern. Subclasses configure criteria,
/// includes, ordering, and paging via protected helper methods.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public abstract class Specification<T> : ISpecification<T>
{
    public Expression<Func<T, bool>>? Criteria { get; private set; }
    public List<Expression<Func<T, object>>> Includes { get; } = new();
    public List<string> IncludeStrings { get; } = new();
    public Expression<Func<T, object>>? OrderBy { get; private set; }
    public Expression<Func<T, object>>? OrderByDescending { get; private set; }
    public Expression<Func<T, object>>? GroupBy { get; private set; }
    public int Take { get; private set; }
    public int Skip { get; private set; }
    public bool IsPagingEnabled { get; private set; }
    public bool CacheEnabled { get; private set; }
    public string? CacheKey { get; private set; }
    public bool IsAsNoTracking { get; private set; } = true;

    protected void ApplyCriteria(Expression<Func<T, bool>> criteria) => Criteria = criteria;

    protected void AddInclude(Expression<Func<T, object>> includeExpression) => Includes.Add(includeExpression);

    protected void AddInclude(string includeString) => IncludeStrings.Add(includeString);

    protected void ApplyOrderBy(Expression<Func<T, object>> orderBy) => OrderBy = orderBy;

    protected void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescending) => OrderByDescending = orderByDescending;

    protected void ApplyGroupBy(Expression<Func<T, object>> groupBy) => GroupBy = groupBy;

    protected void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
        IsPagingEnabled = true;
    }

    protected void EnableCaching(string cacheKey)
    {
        CacheKey = cacheKey;
        CacheEnabled = true;
    }

    protected void AsTracking() => IsAsNoTracking = false;
}
