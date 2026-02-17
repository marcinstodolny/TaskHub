using TaskHub.Domain.Base;

namespace TaskHub.Domain.ValueObjects
{
    public sealed class TaskItemTitle
    {
        private TaskItemTitle(string value)
        {
            Value = value;
        }

        public const int MaxLength = 200;
        public string Value { get; private set; }

        public static Result<TaskItemTitle> Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return Result.Fail<TaskItemTitle>("Title cannot be empty or whitespace.");

            var trimmed = value.Trim();

            return trimmed.Length > MaxLength ? Result.Fail<TaskItemTitle>($"Title cannot exceed {MaxLength} characters.") : Result.Success(new TaskItemTitle(trimmed));
        }
    }
}