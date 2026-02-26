using TaskStatus = TaskHub.Domain.Enums.TaskStatus;

namespace TaskHub.Web.ApiClient.Contracts;

public sealed record UpdateTaskStatusDto(TaskStatus Status);
