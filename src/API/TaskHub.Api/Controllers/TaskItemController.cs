using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskHub.Application.Features.TaskItem.Commands;
using TaskHub.Application.Features.TaskItem.Queries;
using TaskHub.Contracts.Common;
using TaskHub.Contracts.TaskItem;

namespace TaskHub.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TaskItemController(IMediator mediator) : ControllerBase
    {
        [HttpGet("{taskItemId:guid}")]
        public async Task<ActionResult<TaskCardResponse>> GetTaskItemById(Guid taskItemId, CancellationToken ct)
        {
            var taskItemResult = await mediator.Send(new GetTaskItemByIdQuery(taskItemId), ct);

            return taskItemResult.IsFailed
                ? this.ToProblem(taskItemResult, StatusCodes.Status404NotFound, "Task item not found")
                : Ok(taskItemResult.Value.ToContract());
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<TaskItemLightResponse>>> GetTaskItems([FromQuery] GetTaskItemsQuery query, CancellationToken ct)
        {
            var taskItemsResult = await mediator.Send(query, ct);

            return taskItemsResult.IsFailed
                ? this.ToProblem(taskItemsResult, StatusCodes.Status404NotFound, "Tasks not found")
                : Ok(taskItemsResult.Value.Map(item => item.ToLightContract()));
        }

        [HttpGet("board/{taskListId:guid}")]
        public async Task<ActionResult<IReadOnlyCollection<TaskCardResponse>>> GetTaskItemsBoard(Guid taskListId, CancellationToken ct)
        {
            var result = await mediator.Send(new GetTaskItemsBoardQuery(taskListId), ct);
            var statusCode = result.IsNotFoundError() ? StatusCodes.Status404NotFound : StatusCodes.Status400BadRequest;
            return result.IsFailed
                ? this.ToProblem(result, statusCode, "Unable to load task board")
                : Ok(result.Value.Select(item => item.ToContract()));
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> CreateTaskItem([FromBody] CreateTaskItemRequest request, CancellationToken ct)
        {
            var task = await mediator.Send(new CreateTaskItemCommand(request.TaskListId, request.Title, request.Description, request.Priority), ct);
            var statusCode = task.IsNotFoundError() ? StatusCodes.Status404NotFound : StatusCodes.Status400BadRequest;

            return task.IsFailed
                ? this.ToProblem(task, statusCode, "Unable to create task item")
                : Ok(task.Value);
        }

        [HttpPost("{taskItemId:guid}/status")]
        public async Task<IActionResult> UpdateTaskStatus(Guid taskItemId, [FromBody] UpdateTaskStatusRequest request, CancellationToken ct)
        {
            var result = await mediator.Send(new UpdateTaskItemStatusCommand(taskItemId, request.Status), ct);
            if (result.IsSuccess) return Ok();

            var statusCode = result.Errors.Any(error => error.Contains("not found", StringComparison.OrdinalIgnoreCase))
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return this.ToProblem(result, statusCode, "Unable to update task status");

        }

        [HttpPut("{taskItemId:guid}")]
        public async Task<ActionResult<TaskCardResponse>> UpdateTaskItem(Guid taskItemId, [FromBody] UpdateTaskItemRequest request, CancellationToken ct)
        {
            var result = await mediator.Send(new UpdateTaskItemCommand(taskItemId, request.Title, request.Description, request.Priority), ct);
            var statusCode = result.IsNotFoundError() ? StatusCodes.Status404NotFound : StatusCodes.Status400BadRequest;
            return result.IsFailed
                ? this.ToProblem(result, statusCode, "Unable to update task item")
                : Ok(result.Value.ToContract());
        }

        [HttpDelete("{taskItemId:guid}")]
        public async Task<IActionResult> DeleteTaskItem(Guid taskItemId, CancellationToken ct)
        {
            var result = await mediator.Send(new DeleteTaskItemCommand(taskItemId), ct);
            var statusCode = result.IsNotFoundError() ? StatusCodes.Status404NotFound : StatusCodes.Status400BadRequest;
            return result.IsFailed
                ? this.ToProblem(result, statusCode, "Unable to delete task item")
                : Ok();
        }
    }
}
