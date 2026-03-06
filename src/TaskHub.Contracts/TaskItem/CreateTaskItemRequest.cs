using TaskPriority = TaskHub.Domain.Enums.TaskPriority;

namespace TaskHub.Contracts.TaskItem;

public sealed record CreateTaskItemRequest(Guid TaskListId, string Title, string? Description, TaskPriority Priority);
