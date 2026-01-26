using FluentResults;

namespace TaskHub.Domain.ValueObjects
{
    public sealed class TaskListTitle
    {
        private TaskListTitle(string value)
        {
            Value = value;
        }

        public const int MaxLength = 100;
        public string Value { get; private set; }

        public static Result<TaskListTitle> Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return Result.Fail<TaskListTitle>("Title cannot be empty or whitespace.");

            var trimmed = value.Trim();

            return trimmed.Length > MaxLength ? Result.Fail<TaskListTitle>($"Title cannot exceed {MaxLength} characters.") : Result.Ok(new TaskListTitle(trimmed));
        }
    }
}