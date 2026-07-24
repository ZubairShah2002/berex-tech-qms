namespace BerexQms.Application.Common;

/// <summary>
/// Parameters for paginated queries. Provides sensible defaults for page number,
/// page size, and optional sorting.
/// </summary>
public sealed record PaginationParams
{
    private const int DefaultPage = 1;
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;

    /// <summary>
    /// The page number (1-based). Defaults to 1.
    /// </summary>
    public int Page { get; init; } = DefaultPage;

    /// <summary>
    /// The maximum number of items per page. Defaults to 20, capped at 100.
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        init => _pageSize = Math.Min(value > 0 ? value : DefaultPageSize, MaxPageSize);
    }
    private readonly int _pageSize = DefaultPageSize;

    /// <summary>
    /// The property name to sort by. Null means use default ordering.
    /// </summary>
    public string? SortBy { get; init; }

    /// <summary>
    /// The sort direction. Defaults to <see cref="Common.SortDirection.Ascending"/>.
    /// </summary>
    public SortDirection SortDirection { get; init; } = SortDirection.Ascending;
}
