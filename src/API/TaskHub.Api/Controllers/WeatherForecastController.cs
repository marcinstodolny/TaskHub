using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskHub.Application.Queries;
using TaskHub.Domain.Entities;

namespace TaskHub.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IMediator _mediator;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpGet("{taskListId:guid}/tasks")]
        public async Task<ActionResult<IReadOnlyCollection<TaskItem>>> GetTasks(Guid taskListId, CancellationToken ct)
        {
            var tasks = await _mediator.Send(new GetTaskListQuery(taskListId), ct);
  
            return tasks.IsFailed ? NotFound(tasks) : Ok(tasks);
        }
    }
}
