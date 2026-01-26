using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskHub.Application.Features.TaskItem.Commands;
using TaskHub.Application.Features.TaskItem.Request;
using TaskHub.Application.Features.TaskItem.Response;

namespace TaskHub.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskItemController(IMediator mediator) : ControllerBase
    {
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
