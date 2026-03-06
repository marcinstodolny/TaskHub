using TaskStatus = TaskHub.Domain.Enums.TaskStatus;

namespace TaskHub.Contracts.TaskItem;

public sealed record UpdateTaskStatusRequest(TaskStatus Status);
