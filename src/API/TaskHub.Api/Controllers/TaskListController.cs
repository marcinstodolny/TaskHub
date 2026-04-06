using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskHub.Application.Features.TaskList.Commands;
using TaskHub.Application.Features.TaskList.Queries;
using TaskHub.Contracts.Common;
using TaskHub.Contracts.TaskList;

namespace TaskHub.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TaskListController(IMediator mediator) : ControllerBase
    {
        [HttpGet("{taskListId:guid}")]
        public async Task<ActionResult<TaskListLightResponse>> GetTaskListById(Guid taskListId, CancellationToken ct)
        {
            var taskListResult = await mediator.Send(new GetTaskListByIdQuery(taskListId), ct);

            return taskListResult.IsFailed
                ? this.ToProblem(taskListResult, StatusCodes.Status404NotFound, "Task list not found")
                : Ok(taskListResult.Value.ToContract());
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<TaskListLightResponse>>> GetTaskLists([FromQuery] GetTaskListsQuery query, CancellationToken ct)
        {
            var taskListsResult = await mediator.Send(query, ct);

            return taskListsResult.IsFailed
                ? this.ToProblem(taskListsResult, StatusCodes.Status404NotFound, "Task lists not found")
                : Ok(taskListsResult.Value.Map(list => list.ToContract()));
        }

        [HttpPost]
        public async Task<ActionResult<TaskListLightResponse>> CreateTaskList([FromBody] CreateTaskListRequest request, CancellationToken ct)
        {
            var taskListResult = await mediator.Send(new CreateTaskListCommand(request.Title), ct);

            return taskListResult.IsFailed
                ? this.ToProblem(taskListResult, StatusCodes.Status400BadRequest, "Unable to create task list")
                : Ok(taskListResult.Value.ToContract());
        }

        [HttpPut("{taskListId:guid}")]
        public async Task<ActionResult<TaskListLightResponse>> UpdateTaskList(Guid taskListId, [FromBody] UpdateTaskListRequest request, CancellationToken ct)
        {
            var result = await mediator.Send(new UpdateTaskListCommand(taskListId, request.Title), ct);
            var statusCode = result.IsNotFoundError() ? StatusCodes.Status404NotFound : StatusCodes.Status400BadRequest;
            return result.IsFailed
                ? this.ToProblem(result, statusCode, "Unable to update task list")
                : Ok(result.Value.ToContract());
        }

        [HttpDelete("{taskListId:guid}")]
        public async Task<IActionResult> DeleteTaskList(Guid taskListId, CancellationToken ct)
        {
            var result = await mediator.Send(new DeleteTaskListCommand(taskListId), ct);
            var statusCode = result.IsNotFoundError() ? StatusCodes.Status404NotFound : StatusCodes.Status400BadRequest;
            return result.IsFailed
                ? this.ToProblem(result, statusCode, "Unable to delete task list")
                : Ok();
        }
    }
}
