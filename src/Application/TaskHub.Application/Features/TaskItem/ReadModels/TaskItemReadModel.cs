using TaskHub.Domain.Enums;
using TaskStatus = TaskHub.Domain.Enums.TaskStatus;

namespace TaskHub.Application.Features.TaskItem.ReadModels
{
    public record TaskItemSummaryReadModel
    {
        public Guid Id { get; set; }
        public Guid TaskListId { get; set; }
        public string Title { get; set; } = string.Empty;
    };

    public sealed record TaskItemReadModel : TaskItemSummaryReadModel
    {
        public string? Description { get; set; }
        public TaskPriority Priority { get; set; }
        public TaskStatus Status { get; set; }
        public IReadOnlyCollection<TaskStatus> AllowedTargetStatuses { get; set; } = [];
    };
}
