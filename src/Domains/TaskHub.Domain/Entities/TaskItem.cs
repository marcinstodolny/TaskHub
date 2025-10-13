using FluentResults;
using TaskHub.Domain.Abstraction;
using TaskHub.Domain.Enums;
using TaskHub.Domain.ValueObjects;
using TaskStatus = TaskHub.Domain.Enums.TaskStatus;

namespace TaskHub.Domain.Entities
{
    public class TaskItem : Entity<Guid>
    {
        public Guid TaskListId { get; private set; }
        public TaskTitle Title { get; private set; }
        public TaskDescription? Description { get; private set; }
        public TaskPriority Priority { get; private set; }
        public TaskStatus Status { get; private set; }

        public TaskItem() { }
        private TaskItem(Guid id, Guid taskListId, TaskTitle title, TaskDescription? description, TaskPriority priority, TaskStatus status) : base(id)
        {
            TaskListId = taskListId;
            Title = title;
            Description = description;
            Priority = priority;
            Status = status;
        }

        public static Result<TaskItem> Create(Guid taskListId, TaskTitle title, TaskDescription? description, TaskPriority priority)
        {
            if (string.IsNullOrWhiteSpace(title?.Value))
                return Result.Fail<TaskItem>("Title is required.");

            var task = new TaskItem(Guid.NewGuid(), taskListId, title, description, priority, TaskStatus.Todo);

            task.Raise(new TaskCreated(task.Id));
            return Result.Ok(task);
        }

        public Result UpdateTitle(TaskTitle? title)
        {
            if (string.IsNullOrWhiteSpace(title?.Value))
                return Result.Fail("Title is required.");

            Title = title;

            Raise(new TaskUpdated(Id));
            return Result.Ok();
        }

        public Result Start(DateTime nowUtc)
        {
            switch (Status)
            {
                case TaskStatus.Done or TaskStatus.Cancelled:
                    return Result.Fail($"Cannot start a task in status {Status}.");
                case TaskStatus.InProgress:
                    return Result.Fail("Task is already in the requested status.");
            }

            Status = TaskStatus.InProgress;
            UpdatedAt = nowUtc;
            return Result.Ok();
        }


        public Result Complete(DateTime nowUtc)
        {
            switch (Status)
            {
                case TaskStatus.Cancelled:
                    return Result.Fail("Cannot complete a cancelled task.");
                case TaskStatus.Done:
                    return Result.Fail("Task is already in the requested status.");
            }

            Status = TaskStatus.Done;
            UpdatedAt = nowUtc;
            return Result.Ok();
        }


        public Result Cancel(DateTime nowUtc)
        {
            switch (Status)
            {
                case TaskStatus.Done:
                    return Result.Fail("Cannot cancel a completed task.");
                case TaskStatus.Cancelled:
                    return Result.Fail("Task is already in the requested status.");
            }

            Status = TaskStatus.Cancelled;
            UpdatedAt = nowUtc;
            return Result.Ok();
        }
    }
}
