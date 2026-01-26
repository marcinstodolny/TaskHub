using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskHub.Application.Commands;
using TaskHub.Application.Requests;
using TaskHub.Application.Response;

namespace TaskHub.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskItemController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TaskItemController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> CreateTaskItem(CreateTaskItemCommand command, CancellationToken ct)
        {
            var task = await _mediator.Send(command, ct);

            return task.IsFailed ? BadRequest(task.Errors) : Ok(task.Value);
        }

        [HttpPost("{taskItemId:guid}/status")]
        public async Task<IActionResult> UpdateTaskStatus(Guid taskItemId, [FromBody] UpdateTaskStatusRequest request, CancellationToken ct)
        {
            var result = await _mediator.Send(new UpdateTaskItemStatusCommand(taskItemId, request.Status), ct);
            return result.IsFailed ? BadRequest(result.Errors) : Ok();
        }

        [HttpPut("{taskItemId:guid}")]
        public async Task<ActionResult<TaskItemResponse>> UpdateTaskItem(Guid taskItemId, [FromBody] UpdateTaskItemRequest request, CancellationToken ct)
        {
            var result = await _mediator.Send(new UpdateTaskItemCommand(taskItemId, request.Title, request.Description, request.Priority), ct);
            return result.IsFailed ? BadRequest(result.Errors) : Ok(result.Value);
        }

        [HttpDelete("{taskItemId:guid}")]
        public async Task<IActionResult> DeleteTaskItem(Guid taskItemId, CancellationToken ct)
        {
            var result = await _mediator.Send(new DeleteTaskItemCommand(taskItemId), ct);
            return result.IsFailed ? BadRequest(result.Errors) : Ok();
        }
    }
}
