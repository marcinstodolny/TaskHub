using TaskHub.Contracts.TaskActivity;

namespace TaskHub.Contracts.Dashboard;

public sealed record DashboardSummaryResponse
{
    public int TaskListCount { get; init; }
    public int TotalTaskCount { get; init; }
    public int TodoCount { get; init; }
    public int InProgressCount { get; init; }
    public int DoneCount { get; init; }
    public int CancelledCount { get; init; }
    public IReadOnlyCollection<TaskActivityResponse> RecentActivities { get; init; } = [];
}
