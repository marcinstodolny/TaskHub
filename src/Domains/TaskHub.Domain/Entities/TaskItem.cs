using TaskHub.Domain.Abstractions;
using TaskHub.Domain.Base;
using TaskHub.Domain.Enums;
using TaskHub.Domain.Policies;
using TaskHub.Domain.ValueObjects;
using TaskStatus = TaskHub.Domain.Enums.TaskStatus;

namespace TaskHub.Domain.Entities
{
    public class TaskItem : Entity<Guid>
    {
        public Guid TaskListId { get; private set; }
        public TaskItemTitle Title { get; private set; }
        public TaskDescription? Description { get; private set; }
        public TaskPriority Priority { get; private set; }
        public TaskStatus Status { get; private set; }

        public TaskItem() { }
        private TaskItem(Guid id, Guid taskListId, TaskItemTitle title, TaskDescription? description, TaskPriority priority, TaskStatus status) : base(id)
        {
            TaskListId = taskListId;
            Title = title;
            Description = description;
            Priority = priority;
            Status = status;
        }

        public static Result<TaskItem> Create(Guid taskListId, TaskItemTitle title, TaskDescription? description, TaskPriority priority)
        {
            if (string.IsNullOrWhiteSpace(title?.Value))
                return Result.Fail<TaskItem>("Title is required.");

            var task = new TaskItem(Guid.NewGuid(), taskListId, title, description, priority, TaskStatus.Todo);

            task.Raise(new TaskCreated(task.Id));
            return Result.Success(task);
        }

        public Result UpdateTitle(TaskItemTitle? title)
        {
            if (string.IsNullOrWhiteSpace(title?.Value))
                return Result.Fail("Title is required.");

            Title = title;

            Raise(new TaskUpdated(Id));
            return Result.Success();
        }

        public Result Update(TaskItemTitle title, TaskDescription? description, TaskPriority priority, DateTime nowUtc)
        {
            if (string.IsNullOrWhiteSpace(title?.Value))
                return Result.Fail("Title is required.");

            Title = title;
            Description = description;
            Priority = priority;
            UpdatedAt = nowUtc;

            Raise(new TaskUpdated(Id));
            return Result.Success();
        }

        public Result UpdateStatus(TaskStatus targetStatus, DateTime nowUtc)
        {
            if (Status == targetStatus)
            {
                return Result.Fail("Task is already in the requested status.");
            }

            if (!TaskStatusTransitionPolicy.CanTransition(Status, targetStatus))
            {
                return Result.Fail($"Cannot move task from {Status} to {targetStatus}.");
            }

            Status = targetStatus;
            UpdatedAt = nowUtc;
            return Result.Success();
        }

        public IReadOnlyCollection<TaskStatus> GetAllowedTargetStatuses() =>
            TaskStatusTransitionPolicy.GetAllowedTargetStatuses(Status);
    }
}
