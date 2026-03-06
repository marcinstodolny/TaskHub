using TaskPriority = TaskHub.Domain.Enums.TaskPriority;

namespace TaskHub.Contracts.TaskItem;

public sealed record UpdateTaskItemRequest(string Title, string? Description, TaskPriority Priority);
