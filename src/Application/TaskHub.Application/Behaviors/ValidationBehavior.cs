using FluentValidation;
using MediatR;
using TaskHub.Domain.Common;

namespace TaskHub.Application.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);
        var failures = (await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, cancellationToken))))
            .SelectMany(result => result.Errors)
            .Where(error => error is not null)
            .Select(error => error.ErrorMessage)
            .Distinct()
            .ToArray();

        if (failures.Length == 0)
        {
            return await next();
        }

        if (typeof(TResponse) == typeof(Result))
        {
            return (TResponse)(object)Result.Fail(failures);
        }

        var valueType = typeof(TResponse).GenericTypeArguments.Single();
        var failMethod = typeof(Result)
            .GetMethods()
            .Single(method => method.Name == nameof(Result.Fail)
                              && method.IsGenericMethodDefinition
                              && method.GetParameters().Length == 1
                              && method.GetParameters()[0].ParameterType == typeof(IReadOnlyCollection<string>));

        var genericFailMethod = failMethod.MakeGenericMethod(valueType);
        var result = genericFailMethod.Invoke(null, [failures]);

        return result is null
            ? throw new InvalidOperationException($"Unable to create validation result for {typeof(TResponse).Name}.")
            : (TResponse)result;
    }
}
