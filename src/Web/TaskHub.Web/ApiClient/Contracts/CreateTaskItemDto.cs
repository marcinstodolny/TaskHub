using TaskPriority = TaskHub.Domain.Enums.TaskPriority;

namespace TaskHub.Web.ApiClient.Contracts;

public sealed record CreateTaskItemDto(Guid TaskListId, string Title, string? Description, TaskPriority Priority);
