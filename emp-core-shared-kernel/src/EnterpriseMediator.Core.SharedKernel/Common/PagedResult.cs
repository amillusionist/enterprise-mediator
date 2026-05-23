namespace EnterpriseMediator.Core.SharedKernel.Common;

/// <summary>
/// Represents a paginated result set for list queries.
/// </summary>
/// <typeparam name="T">The type of items in the page.</typeparam>
public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; }
    public int TotalCount { get; }
    public int Page { get; }
    public int PageSize { get; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;

    public PagedResult(IReadOnlyList<T> items, int totalCount, int page, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        Page = page;
        PageSize = pageSize;
    }

    public static PagedResult<T> Empty(int page, int pageSize) =>
        new(Array.Empty<T>(), 0, page, pageSize);
}
