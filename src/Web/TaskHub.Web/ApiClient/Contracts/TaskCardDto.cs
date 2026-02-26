using TaskStatus = TaskHub.Domain.Enums.TaskStatus;

namespace TaskHub.Web.ApiClient.Contracts;

public sealed record TaskCardDto
{
    public Guid Id { get; set; }
    public Guid TaskListId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskStatus Status { get; set; }
}
