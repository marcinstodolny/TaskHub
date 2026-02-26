using Microsoft.AspNetCore.Mvc;
using TaskHub.Domain.Base;

namespace TaskHub.Api.Controllers;

internal static class ControllerBaseExtensions
{
    public static ObjectResult ToProblem(this ControllerBase controller, Result result, int statusCode, string title)
    {
        return controller.Problem(
            statusCode: statusCode,
            title: title,
            detail: string.Join(" ", result.Errors));
    }

    public static ObjectResult ToProblem<T>(this ControllerBase controller, Result<T> result, int statusCode, string title)
    {
        return controller.Problem(
            statusCode: statusCode,
            title: title,
            detail: string.Join(" ", result.Errors));
    }
}
