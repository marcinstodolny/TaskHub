namespace TaskHub.Contracts.Common;

public sealed record PaginatedResponse<TItem>
{
    public IReadOnlyCollection<TItem> Items { get; init; } = [];
    public required int CurrentPage { get; init; }
    public required int TotalPageCount { get; init; }
}
