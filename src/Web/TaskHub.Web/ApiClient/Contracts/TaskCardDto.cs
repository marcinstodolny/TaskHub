using TaskStatus = TaskHub.Domain.Enums.TaskStatus;
using TaskPriority = TaskHub.Domain.Enums.TaskPriority;

namespace TaskHub.Web.ApiClient.Contracts;

public sealed record TaskCardDto
{
    public Guid Id { get; set; }
    public Guid TaskListId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskPriority Priority { get; set; }
    public TaskStatus Status { get; set; }
    public IReadOnlyCollection<TaskStatus> AllowedTargetStatuses { get; set; } = [];
}
