using TaskHub.Application.Features.TaskItem.Response;

namespace TaskHub.Application.Features.TaskList.Response
{
    public sealed record TaskListResponse()
    {
        public Guid Id { get; set; }
        public string Title { get; set; }

        public List<TaskItemResponse> Tasks { get; set; } = new();
    };
    
}
