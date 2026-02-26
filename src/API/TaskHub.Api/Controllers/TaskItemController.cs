using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskHub.Application.Base.Response;
using TaskHub.Application.Features.TaskItem.Commands;
using TaskHub.Application.Features.TaskItem.Queries;
using TaskHub.Application.Features.TaskItem.Request;
using TaskHub.Application.Features.TaskItem.Response;

namespace TaskHub.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskItemController(IMediator mediator) : ControllerBase
    {
        [HttpGet("{taskItemId:guid}")]
        public async Task<ActionResult<TaskItemResponse>> GetTaskItemById(Guid taskItemId, CancellationToken ct)
        {
            var taskItemResult = await mediator.Send(new GetTaskItemByIdQuery(taskItemId), ct);

            return taskItemResult.IsFailed
                ? this.ToProblem(taskItemResult, StatusCodes.Status404NotFound, "Task item not found")
                : Ok(taskItemResult.Value);
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<TaskItemResponse>>> GetTaskItems([FromQuery] GetTaskItemsQuery query, CancellationToken ct)
        {
            var taskItemsResult = await mediator.Send(query, ct);

            return taskItemsResult.IsFailed
                ? this.ToProblem(taskItemsResult, StatusCodes.Status404NotFound, "Tasks not found")
                : Ok(taskItemsResult.Value);
        }

        [HttpGet("board/{taskListId:guid}")]
        public async Task<ActionResult<IReadOnlyCollection<TaskItemResponse>>> GetTaskItemsBoard(Guid taskListId, CancellationToken ct)
        {
            var result = await mediator.Send(new GetTaskItemsBoardQuery(taskListId), ct);
            return result.IsFailed
                ? this.ToProblem(result, StatusCodes.Status400BadRequest, "Unable to load task board")
                : Ok(result.Value);
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> CreateTaskItem(CreateTaskItemCommand command, CancellationToken ct)
        {
            var task = await mediator.Send(command, ct);

            return task.IsFailed
                ? this.ToProblem(task, StatusCodes.Status400BadRequest, "Unable to create task item")
                : Ok(task.Value);
        }

        [HttpPost("{taskItemId:guid}/status")]
        public async Task<IActionResult> UpdateTaskStatus(Guid taskItemId, [FromBody] UpdateTaskStatusRequest request, CancellationToken ct)
        {
            var result = await mediator.Send(new UpdateTaskItemStatusCommand(taskItemId, request.Status), ct);
            return result.IsFailed
                ? this.ToProblem(result, StatusCodes.Status400BadRequest, "Unable to update task status")
                : Ok();
        }

        [HttpPut("{taskItemId:guid}")]
        public async Task<ActionResult<TaskItemResponse>> UpdateTaskItem(Guid taskItemId, [FromBody] UpdateTaskItemRequest request, CancellationToken ct)
        {
            var result = await mediator.Send(new UpdateTaskItemCommand(taskItemId, request.Title, request.Description, request.Priority), ct);
            return result.IsFailed
                ? this.ToProblem(result, StatusCodes.Status400BadRequest, "Unable to update task item")
                : Ok(result.Value);
        }

        [HttpDelete("{taskItemId:guid}")]
        public async Task<IActionResult> DeleteTaskItem(Guid taskItemId, CancellationToken ct)
        {
            var result = await mediator.Send(new DeleteTaskItemCommand(taskItemId), ct);
            return result.IsFailed
                ? this.ToProblem(result, StatusCodes.Status400BadRequest, "Unable to delete task item")
                : Ok();
        }
    }
}
