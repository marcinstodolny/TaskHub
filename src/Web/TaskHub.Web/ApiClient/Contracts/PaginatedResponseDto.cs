namespace TaskHub.Web.ApiClient.Contracts;

public sealed record PaginatedResponseDto<TItem>
{
    public List<TItem> Items { get; set; } = [];
}
