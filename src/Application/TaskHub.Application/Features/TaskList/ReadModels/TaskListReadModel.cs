using TaskHub.Application.Features.TaskItem.ReadModels;

namespace TaskHub.Application.Features.TaskList.ReadModels
{
    public abstract record TaskListReadModelBase
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = string.Empty;
    }

    public sealed record TaskListSummaryReadModel : TaskListReadModelBase
    {
        public int TasksCount { get; init; } = 0;
    }

    public sealed record TaskListDetailsReadModel : TaskListReadModelBase
    {
        public int TasksCount { get; init; } = 0;
        public List<TaskItemReadModel> Tasks { get; init; } = [];
    }
}
