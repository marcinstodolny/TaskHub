using TaskHub.Domain.Enums;

namespace TaskHub.Contracts.TaskActivity;

public sealed record TaskActivityResponse
{
    public Guid Id { get; init; }
    public Guid TaskListId { get; init; }
    public Guid? TaskItemId { get; init; }
    public TaskActivityType Type { get; init; }
    public string Message { get; init; } = string.Empty;
    public DateTime CreatedAtUtc { get; init; }
    public string? TaskTitleSnapshot { get; init; }
}
