using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TaskHub.Application.abstraction;

namespace TaskHub.Api.Filters;

public sealed class RequireAuthSchemaAvailableFilter(IAuthSchemaAvailabilityChecker authSchemaAvailabilityChecker)
    : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (await authSchemaAvailabilityChecker.IsUsersSchemaAvailableAsync(context.HttpContext.RequestAborted))
        {
            await next();
            return;
        }

        context.Result = new ObjectResult(new ProblemDetails
        {
            Status = StatusCodes.Status503ServiceUnavailable,
            Title = "Authentication is unavailable",
            Detail = "The users schema is not available yet. Apply database migrations and try again."
        })
        {
            StatusCode = StatusCodes.Status503ServiceUnavailable
        };
    }
}
