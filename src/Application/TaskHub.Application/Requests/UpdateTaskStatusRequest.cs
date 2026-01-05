using TaskStatus = TaskHub.Domain.Enums.TaskStatus;

namespace TaskHub.Application.Requests
{
    public sealed record UpdateTaskStatusRequest(TaskStatus Status);
}
