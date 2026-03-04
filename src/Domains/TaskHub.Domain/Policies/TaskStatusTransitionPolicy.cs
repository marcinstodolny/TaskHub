using TaskStatus = TaskHub.Domain.Enums.TaskStatus;

namespace TaskHub.Domain.Policies;

public static class TaskStatusTransitionPolicy
{
    public static IReadOnlyCollection<TaskStatus> GetAllowedTargetStatuses(TaskStatus currentStatus) => currentStatus switch
    {
        TaskStatus.Todo => [TaskStatus.InProgress, TaskStatus.Done, TaskStatus.Cancelled],
        TaskStatus.InProgress => [TaskStatus.Todo, TaskStatus.Done, TaskStatus.Cancelled],
        TaskStatus.Done => [TaskStatus.InProgress],
        TaskStatus.Cancelled => [TaskStatus.Todo],
        _ => []
    };

    public static bool CanTransition(TaskStatus currentStatus, TaskStatus targetStatus) =>
        GetAllowedTargetStatuses(currentStatus).Contains(targetStatus);
}
