namespace TaskHub.Application.Common.Pagination;

public sealed record PagedResult<T>
{
    public required IReadOnlyCollection<T> Items { get; init; }
    public required int PageNumber { get; init; }
    public required int TotalPages { get; init; }
}
