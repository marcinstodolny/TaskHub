namespace TaskHub.Contracts.TaskList;

public sealed record TaskListLightResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int TasksCount { get; set; }
}
