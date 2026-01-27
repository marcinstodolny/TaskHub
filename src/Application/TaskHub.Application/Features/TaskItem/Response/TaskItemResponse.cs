using TaskHub.Domain.Enums;
using TaskStatus = TaskHub.Domain.Enums.TaskStatus;

namespace TaskHub.Application.Features.TaskItem.Response
{
    public record TaskItemLightResponse
    {
        public Guid Id { get; set; }
        public Guid TaskListId { get; set; }
        public string Title { get; set; }
    };

    public sealed record TaskItemResponse : TaskItemLightResponse
    {
        public string? Description { get; set; }
        public TaskPriority Priority { get; set; }
        public TaskStatus Status { get; set; }
    };
}
