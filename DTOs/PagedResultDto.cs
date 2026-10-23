namespace AdvancedEFCoreAPI.DTOs;

public sealed class PagedResultDto<T>
{
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public IReadOnlyList<T> Items { get; init; } = [];
}
