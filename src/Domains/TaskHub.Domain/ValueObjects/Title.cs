using FluentResults;

namespace TaskHub.Domain.ValueObjects
{
    public class Title
    {
        private Title(string value)
        {
            Value = value;
        }

        private const int MaxLength = 100;
        public string Value { get; private set; }


        public static Result<Title> Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return Result.Fail<Title>("Title cannot be empty or whitespace.");

            var trimmed = value.Trim();

            if (trimmed.Length > MaxLength)
                return Result.Fail<Title>($"Title cannot exceed {MaxLength} characters.");

            return Result.Ok(new Title(trimmed));
        }
    }
}
