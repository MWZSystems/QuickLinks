using Microsoft.EntityFrameworkCore;

namespace QuickLinks.Models.ViewModels.Admin;

public sealed class PagedList<T>
{
    public required IReadOnlyList<T> Items { get; init; }

    public required ListQuery Query { get; init; }

    public int TotalCount { get; init; }

    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)ListQuery.PageSize));

    public static async Task<PagedList<T>> CreateAsync(IQueryable<T> source, ListQuery query, CancellationToken ct)
    {
        var total = await source.CountAsync(ct);
        var pages = Math.Max(1, (int)Math.Ceiling(total / (double)ListQuery.PageSize));
        query.Page = Math.Clamp(query.Page, 1, pages);

        var items = await source
            .Skip((query.Page - 1) * ListQuery.PageSize)
            .Take(ListQuery.PageSize)
            .ToListAsync(ct);

        return new PagedList<T> { Items = items, Query = query, TotalCount = total };
    }
}
