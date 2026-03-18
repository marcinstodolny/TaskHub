namespace TaskHub.Contracts.TaskItem;

public sealed record TaskItemLightResponse
{
    public Guid Id { get; set; }
    public Guid TaskListId { get; set; }
    public string Title { get; set; } = string.Empty;
}
