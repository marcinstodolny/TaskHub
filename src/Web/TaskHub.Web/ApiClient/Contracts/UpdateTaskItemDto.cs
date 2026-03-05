using TaskPriority = TaskHub.Domain.Enums.TaskPriority;

namespace TaskHub.Web.ApiClient.Contracts;

public sealed record UpdateTaskItemDto(string Title, string? Description, TaskPriority Priority);
