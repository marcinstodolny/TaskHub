namespace TaskHub.Web.ApiClient.Contracts;

public sealed record TaskListLightDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
}
