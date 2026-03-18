using TaskPriority = TaskHub.Domain.Enums.TaskPriority;
using TaskStatus = TaskHub.Domain.Enums.TaskStatus;

namespace TaskHub.Contracts.TaskItem;

public sealed record TaskCardResponse
{
    public Guid Id { get; set; }
    public Guid TaskListId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskPriority Priority { get; set; }
    public TaskStatus Status { get; set; }
    public IReadOnlyCollection<TaskStatus> AllowedTargetStatuses { get; set; } = [];
}
