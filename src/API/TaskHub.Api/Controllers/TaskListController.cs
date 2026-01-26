using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskHub.Application.Features.TaskList.Commands;
using TaskHub.Application.Features.TaskList.Queries;
using TaskHub.Application.Features.TaskList.Request;
using TaskHub.Application.Features.TaskList.Response;

namespace TaskHub.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskListController(IMediator mediator) : ControllerBase
    {
        [HttpGet("{taskListId:guid}")]
        public async Task<ActionResult<TaskListResponse>> GetTaskListById(Guid taskListId, CancellationToken ct)
        {
            var taskListResult = await mediator.Send(new GetTaskListByIdQuery(taskListId), ct);
  
            return taskListResult.IsFailed ? NotFound(taskListResult.Errors) : Ok(taskListResult.Value);
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyCollection<TaskListResponse>>> GetTaskLists(CancellationToken ct)
        {
            var taskListsResult = await mediator.Send(new GetTaskListsQuery(), ct);

            return taskListsResult.IsFailed ? NotFound(taskListsResult.Errors) : Ok(taskListsResult.Value);
        }

        [HttpPost]
        public async Task<ActionResult<TaskListResponse>> CreateTaskList(CreateTaskListCommand command, CancellationToken ct)
        {
            var taskListResult = await mediator.Send(command, ct);

            return taskListResult.IsFailed ? BadRequest(taskListResult.Errors) : Ok(taskListResult.Value);
        }

        [HttpPut("{taskListId:guid}")]
        public async Task<ActionResult<TaskListResponse>> UpdateTaskList(Guid taskListId, [FromBody] UpdateTaskListRequest request, CancellationToken ct)
        {
            var result = await mediator.Send(new UpdateTaskListCommand(taskListId, request.Title), ct);
            return result.IsFailed ? BadRequest(result.Errors) : Ok(result.Value);
        }

        [HttpDelete("{taskListId:guid}")]
        public async Task<IActionResult> DeleteTaskList(Guid taskListId, CancellationToken ct)
        {
            var result = await mediator.Send(new DeleteTaskListCommand(taskListId), ct);
            return result.IsFailed ? BadRequest(result.Errors) : Ok();
        }
    }
}
