using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskHub.Application.Features.TaskItem.Commands;
using TaskHub.Application.Features.TaskItem.Queries;
using TaskHub.Application.Features.TaskItem.Request;
using TaskHub.Application.Features.TaskItem.Response;
using TaskHub.Application.Response;

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

            return taskItemResult.IsFailed ? NotFound(taskItemResult.Errors) : Ok(taskItemResult.Value);
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<TaskItemResponse>>> GetTaskItems([FromQuery] GetTaskItemsQuery query, CancellationToken ct)
        {
            var taskItemsResult = await mediator.Send(query, ct);

            return taskItemsResult.IsFailed ? NotFound(taskItemsResult.Errors) : Ok(taskItemsResult.Value);
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> CreateTaskItem(CreateTaskItemCommand command, CancellationToken ct)
        {
            var task = await mediator.Send(command, ct);

            return task.IsFailed ? BadRequest(task.Errors) : Ok(task.Value);
        }

        [HttpPost("{taskItemId:guid}/status")]
        public async Task<IActionResult> UpdateTaskStatus(Guid taskItemId, [FromBody] UpdateTaskStatusRequest request, CancellationToken ct)
        {
            var result = await mediator.Send(new UpdateTaskItemStatusCommand(taskItemId, request.Status), ct);
            return result.IsFailed ? BadRequest(result.Errors) : Ok();
        }

        [HttpPut("{taskItemId:guid}")]
        public async Task<ActionResult<TaskItemResponse>> UpdateTaskItem(Guid taskItemId, [FromBody] UpdateTaskItemRequest request, CancellationToken ct)
        {
            var result = await mediator.Send(new UpdateTaskItemCommand(taskItemId, request.Title, request.Description, request.Priority), ct);
            return result.IsFailed ? BadRequest(result.Errors) : Ok(result.Value);
        }

        [HttpDelete("{taskItemId:guid}")]
        public async Task<IActionResult> DeleteTaskItem(Guid taskItemId, CancellationToken ct)
        {
            var result = await mediator.Send(new DeleteTaskItemCommand(taskItemId), ct);
            return result.IsFailed ? BadRequest(result.Errors) : Ok();
        }
    }
}
