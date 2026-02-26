using Microsoft.AspNetCore.Mvc;
using TaskHub.Domain.Base;

namespace TaskHub.Api.Controllers;

internal static class ControllerBaseExtensions
{
    extension(ControllerBase controller)
    {
        public ObjectResult ToProblem(Result result, int statusCode, string title)
        {
            return controller.Problem(
                statusCode: statusCode,
                title: title,
                detail: string.Join(" ", result.Errors));
        }

        public ObjectResult ToProblem<T>(Result<T> result, int statusCode, string title)
        {
            return controller.Problem(
                statusCode: statusCode,
                title: title,
                detail: string.Join(" ", result.Errors));
        }
    }
}
