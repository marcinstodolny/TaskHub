using TaskHub.Application.Features.TaskItem.Response;

namespace TaskHub.Application.Features.TaskList.Response
{
    public record TaskListLightResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
    };

    public sealed record TaskListResponse : TaskListLightResponse
    {
        public List<TaskItemResponse> Tasks { get; set; } = new();
    };

}
