using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskHub.Application.Commands;
using TaskHub.Application.Queries;
using TaskHub.Application.Requests;
using TaskHub.Application.Response;

namespace TaskHub.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TaskController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{taskListId:guid}")]
        public async Task<ActionResult<TaskListResponse>> GetTaskListById(Guid taskListId, CancellationToken ct)
        {
            var taskListResult = await _mediator.Send(new GetTaskListByIdQuery(taskListId), ct);
  
            return taskListResult.IsFailed ? NotFound(taskListResult.Errors) : Ok(taskListResult.Value);
        }

        [HttpGet("tasklists")]
        public async Task<ActionResult<IReadOnlyCollection<TaskListResponse>>> GetTaskLists(CancellationToken ct)
        {
            var taskListsResult = await _mediator.Send(new GetTaskListsQuery(), ct);

            return taskListsResult.IsFailed ? NotFound(taskListsResult.Errors) : Ok(taskListsResult.Value);
        }

        [HttpPost("list")]
        public async Task<ActionResult<TaskListResponse>> CreateTaskList(CreateTaskListCommand command, CancellationToken ct)
        {
            var taskListResult = await _mediator.Send(command, ct);

            return taskListResult.IsFailed ? BadRequest(taskListResult.Errors) : Ok(taskListResult.Value);
        }

        [HttpPost("item")]
        public async Task<ActionResult<Guid>> CreateTaskItem(CreateTaskItemCommand command, CancellationToken ct)
        {
            var task = await _mediator.Send(command, ct);

            return task.IsFailed ? BadRequest(task.Errors) : Ok(task.Value);
        }

        [HttpPost("item/{taskItemId:guid}/status")]
        public async Task<IActionResult> UpdateTaskStatus(Guid taskItemId, [FromBody] UpdateTaskStatusRequest request, CancellationToken ct)
        {
            var result = await _mediator.Send(new UpdateTaskItemStatusCommand(taskItemId, request.Status), ct);
            return result.IsFailed ? BadRequest(result.Errors) : Ok();
        }
    }
}
