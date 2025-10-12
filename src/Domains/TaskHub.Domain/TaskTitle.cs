using FluentResults;

namespace TaskHub.Domain
{
    public class TaskTitle(string value)
    {
        private const int MaxLength = 100;
        public string Value { get; private set; } = value;

        public static Result<TaskTitle> Create(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return Result.Fail<TaskTitle>("Title cannot be empty or whitespace.");

            var trimmed = value.Trim();

            if (trimmed.Length > MaxLength)
                return Result.Fail<TaskTitle>($"Title cannot exceed {MaxLength} characters.");

            return Result.Ok(new TaskTitle(trimmed));
        }
    }
}
