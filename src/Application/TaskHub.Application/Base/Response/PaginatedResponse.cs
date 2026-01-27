namespace TaskHub.Application.Response
{
    public sealed record PaginatedResponse<T>
    {
        public IReadOnlyCollection<T> Items { get; init; } = new List<T>();
        public required int CurrentPage { get; init; }
        public required int TotalPageCount { get; init; }
    }
}
