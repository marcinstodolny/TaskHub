namespace TaskHub.Application.Response
{
    public sealed record PaginatedResponse<T>
    {
        public required IReadOnlyCollection<T> Items { get; init; }
        public required int CurrentPage { get; init; }
        public required int TotalPageCount { get; init; }
    }
}
