using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskHub.Application.Commands;
using TaskHub.Application.Queries;
using TaskHub.Domain.Entities;

namespace TaskHub.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TaskController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TaskController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{taskListId:guid}/tasks")]
        public async Task<ActionResult<IReadOnlyCollection<TaskItem>>> GetTasks(GetTaskListQuery query, CancellationToken ct)
        {
            var tasks = await _mediator.Send(query, ct);
  
            return tasks.IsFailed ? NotFound(tasks) : Ok(tasks);
        }

        [HttpPost("List")]
        public async Task<ActionResult<IReadOnlyCollection<TaskItem>>> CreateTaskList(CreateTaskListCommand command, CancellationToken ct)
        {
            var tasks = await _mediator.Send(command, ct);

            return tasks.IsFailed ? NotFound(tasks) : Ok(tasks);
        }

        [HttpPost("Item")]
        public async Task<ActionResult<IReadOnlyCollection<TaskItem>>> CreateTaskItem(CreateTaskItemCommand command, CancellationToken ct)
        {
            var tasks = await _mediator.Send(command, ct);

            return tasks.IsFailed ? NotFound(tasks) : Ok(tasks);
        }
    }
}
