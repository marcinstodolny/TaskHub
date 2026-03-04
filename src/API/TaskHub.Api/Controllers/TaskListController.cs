using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskHub.Application.Base.Response;
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

            return taskListResult.IsFailed
                ? this.ToProblem(taskListResult, StatusCodes.Status404NotFound, "Task list not found")
                : Ok(taskListResult.Value);
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<TaskListResponse>>> GetTaskLists([FromQuery] GetTaskListsQuery query, CancellationToken ct)
        {
            var taskListsResult = await mediator.Send(query, ct);

            return taskListsResult.IsFailed
                ? this.ToProblem(taskListsResult, StatusCodes.Status404NotFound, "Task lists not found")
                : Ok(taskListsResult.Value);
        }

        [HttpPost]
        public async Task<ActionResult<TaskListResponse>> CreateTaskList(CreateTaskListCommand command, CancellationToken ct)
        {
            var taskListResult = await mediator.Send(command, ct);

            return taskListResult.IsFailed
                ? this.ToProblem(taskListResult, StatusCodes.Status400BadRequest, "Unable to create task list")
                : Ok(taskListResult.Value);
        }

        [HttpPut("{taskListId:guid}")]
        public async Task<ActionResult<TaskListResponse>> UpdateTaskList(Guid taskListId, [FromBody] UpdateTaskListRequest request, CancellationToken ct)
        {
            var result = await mediator.Send(new UpdateTaskListCommand(taskListId, request.Title), ct);
            return result.IsFailed
                ? this.ToProblem(result, StatusCodes.Status400BadRequest, "Unable to update task list")
                : Ok(result.Value);
        }

        [HttpDelete("{taskListId:guid}")]
        public async Task<IActionResult> DeleteTaskList(Guid taskListId, CancellationToken ct)
        {
            var result = await mediator.Send(new DeleteTaskListCommand(taskListId), ct);
            return result.IsFailed
                ? this.ToProblem(result, StatusCodes.Status400BadRequest, "Unable to delete task list")
                : Ok();
        }
    }
}
