using FluentResults;

namespace TaskHub.Domain.ValueObjects
{
    public class TaskDescription(string value)
    {
        private const int MaxLength = 1000;
        public string Value { get; private set; } = value;

        public static Result<TaskDescription> Create(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return Result.Fail<TaskDescription>("Task description cannot be empty or whitespace.");

            var trimmed = value.Trim();

            if (trimmed.Length > MaxLength)
                return Result.Fail<TaskDescription>($"Description cannot exceed {MaxLength} characters.");

            return Result.Ok(new TaskDescription(trimmed));
        }
    }
}
