using TaskStatus = TaskHub.Domain.Enums.TaskStatus;

namespace TaskHub.Application.Features.TaskItem.Request
{
    public sealed record UpdateTaskStatusRequest(TaskStatus Status);
}
