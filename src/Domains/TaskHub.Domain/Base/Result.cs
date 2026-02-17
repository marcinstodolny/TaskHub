namespace TaskHub.Domain.Base;

public class Result
{
    private const string DefaultErrorMessage = "Unknown error";

    protected Result(bool isSuccess, IReadOnlyCollection<string> errors)
    {
        IsSuccess = isSuccess;
        Errors = NormalizeErrors(isSuccess, errors);
    }

    public bool IsSuccess { get; }
    public bool IsFailed => !IsSuccess;
    public IReadOnlyCollection<string> Errors { get; }

    public static Result Success() => new(true, Array.Empty<string>());

    public static Result Fail(string error) => new(false, new[] { error });

    public static Result Fail(IReadOnlyCollection<string> errors) => new(false, errors);

    public static Result<TValue> Success<TValue>(TValue value) => Result<TValue>.Success(value);

    public static Result<TValue> Fail<TValue>(string error) => Result<TValue>.Fail(error);

    public static Result<TValue> Fail<TValue>(IReadOnlyCollection<string> errors) => Result<TValue>.Fail(errors);

    private static IReadOnlyCollection<string> NormalizeErrors(bool isSuccess, IReadOnlyCollection<string> errors)
    {
        if (!isSuccess)
            return errors?.Count > 0 ? errors.ToArray() : new[] { DefaultErrorMessage };

        return errors?.Count > 0
            ? throw new InvalidOperationException("A successful result cannot have errors.")
            : Array.Empty<string>();
    }
}

public sealed class Result<TValue> : Result
{
    private readonly TValue? _value;

    private Result(bool isSuccess, TValue? value, IReadOnlyCollection<string> errors)
        : base(isSuccess, errors)
    {
        _value = value;
    }

    public TValue Value => IsFailed ? throw new InvalidOperationException("Cannot access the value of a failed result.") : _value!;

    public static Result<TValue> Success(TValue value)
    {
        return value is null
            ? throw new ArgumentNullException(nameof(value), "Success value cannot be null.")
            : new Result<TValue>(true, value, Array.Empty<string>());
    }

    public new static Result<TValue> Fail(string error) => new(false, default, new[] { error });

    public new static Result<TValue> Fail(IReadOnlyCollection<string> errors) => new(false, default, errors);
}