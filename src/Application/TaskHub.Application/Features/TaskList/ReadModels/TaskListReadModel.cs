using TaskHub.Application.Features.TaskItem.ReadModels;

namespace TaskHub.Application.Features.TaskList.ReadModels
{
    public record TaskListSummaryReadModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
    };

    public sealed record TaskListReadModel : TaskListSummaryReadModel
    {
        public List<TaskItemReadModel> Tasks { get; set; } = new();
    };

}
