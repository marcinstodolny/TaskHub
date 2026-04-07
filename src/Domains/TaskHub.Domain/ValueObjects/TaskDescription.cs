using TaskHub.Domain.Common;

namespace TaskHub.Domain.ValueObjects
{
    public class TaskDescription(string value)
    {
        public const int MaxLength = 2000;
        public string Value { get; private set; } = value;

        public static Result<TaskDescription> Create(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return Result.Fail<TaskDescription>("Task description cannot be empty or whitespace.");

            var trimmed = value.Trim();

            return trimmed.Length > MaxLength ? Result.Fail<TaskDescription>($"Description cannot exceed {MaxLength} characters.") : Result.Success(new TaskDescription(trimmed));
        }
    }
}
