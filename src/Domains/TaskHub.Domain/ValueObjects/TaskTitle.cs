using FluentResults;

namespace TaskHub.Domain.ValueObjects
{
    public class TaskTitle
    {
        private TaskTitle(string value)
        {
            Value = value;
        }

        private const int MaxLength = 100;
        public string Value { get; private set; }


        public static Result<TaskTitle> Create(string value)
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
