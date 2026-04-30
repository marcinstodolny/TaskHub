using TaskHub.Domain.Common;
using TaskHub.Domain.Enums;
using TaskHub.Domain.ValueObjects;

namespace TaskHub.Domain.Entities
{
    public sealed class TaskActivity : Entity<Guid>
    {
        public const int MaxMessageLength = 500;

        private TaskActivity() { }

        private TaskActivity(
            Guid id,
            Guid taskListId,
            Guid? taskItemId,
            TaskActivityType type,
            string message,
            DateTime createdAtUtc,
            string? taskTitleSnapshot) : base(id)
        {
            TaskListId = taskListId;
            TaskItemId = taskItemId;
            Type = type;
            Message = message;
            CreatedAtUtc = createdAtUtc;
            TaskTitleSnapshot = taskTitleSnapshot;
        }

        public Guid TaskListId { get; private set; }
        public Guid? TaskItemId { get; private set; }
        public TaskActivityType Type { get; private set; }
        public string Message { get; private set; } = string.Empty;
        public DateTime CreatedAtUtc { get; private set; }
        public string? TaskTitleSnapshot { get; private set; }

        public static Result<TaskActivity> Create(
            Guid taskListId,
            Guid? taskItemId,
            TaskActivityType type,
            string message,
            DateTime createdAtUtc,
            string? taskTitleSnapshot = null)
        {
            if (taskListId == Guid.Empty)
            {
                return Result.Fail<TaskActivity>("Task list id is required.");
            }

            if (taskItemId == Guid.Empty)
            {
                return Result.Fail<TaskActivity>("Task item id cannot be empty.");
            }

            if (!Enum.IsDefined(type))
            {
                return Result.Fail<TaskActivity>("Activity type must be valid.");
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                return Result.Fail<TaskActivity>("Activity message is required.");
            }

            if (createdAtUtc == default)
            {
                return Result.Fail<TaskActivity>("Created date is required.");
            }

            var trimmedMessage = message.Trim();
            if (trimmedMessage.Length > MaxMessageLength)
            {
                return Result.Fail<TaskActivity>($"Activity message cannot exceed {MaxMessageLength} characters.");
            }

            var normalizedTitleSnapshot = NormalizeTaskTitleSnapshot(taskTitleSnapshot);
            if (normalizedTitleSnapshot?.Length > TaskItemTitle.MaxLength)
            {
                return Result.Fail<TaskActivity>($"Task title snapshot cannot exceed {TaskItemTitle.MaxLength} characters.");
            }

            return Result.Success(new TaskActivity(
                Guid.NewGuid(),
                taskListId,
                taskItemId,
                type,
                trimmedMessage,
                createdAtUtc,
                normalizedTitleSnapshot));
        }

        private static string? NormalizeTaskTitleSnapshot(string? taskTitleSnapshot)
        {
            if (string.IsNullOrWhiteSpace(taskTitleSnapshot))
            {
                return null;
            }

            return taskTitleSnapshot.Trim();
        }
    }
}
