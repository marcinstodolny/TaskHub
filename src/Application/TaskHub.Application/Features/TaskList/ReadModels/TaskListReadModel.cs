using TaskHub.Application.Features.TaskItem.ReadModels;

namespace TaskHub.Application.Features.TaskList.ReadModels
{
    public abstract record TaskListReadModelBase
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
    }

    public sealed record TaskListSummaryReadModel : TaskListReadModelBase
    {
        public int TasksCount { get; set; } = 0;
    }

    public sealed record TaskListDetailsReadModel : TaskListReadModelBase
    {
        public List<TaskItemReadModel> Tasks { get; set; } = new();
    }
}
