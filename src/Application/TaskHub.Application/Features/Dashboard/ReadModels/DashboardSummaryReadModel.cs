using TaskHub.Application.Features.TaskActivity.ReadModels;

namespace TaskHub.Application.Features.Dashboard.ReadModels;

public sealed record DashboardSummaryReadModel
{
    public int TaskListCount { get; init; }
    public int TotalTaskCount { get; init; }
    public int TodoCount { get; init; }
    public int InProgressCount { get; init; }
    public int DoneCount { get; init; }
    public int CancelledCount { get; init; }
    public IReadOnlyCollection<TaskActivityReadModel> RecentActivities { get; init; } = [];
}
