using Dapper;
using Microsoft.EntityFrameworkCore;
using TaskHub.Application.Abstractions;
using TaskHub.Application.Abstractions.Repositories.Query;
using TaskHub.Application.Features.Dashboard.ReadModels;
using TaskHub.Application.Features.TaskActivity.ReadModels;
using TaskHub.Infrastructure.Persistence;

namespace TaskHub.Infrastructure.Repositories.Query;

internal sealed class DashboardQueryRepository(TaskHubDbContext db, ICurrentUserAccessor currentUserAccessor)
    : IDashboardQueryRepository
{
    public async Task<DashboardSummaryReadModel> GetSummaryAsync(int recentActivityCount, CancellationToken ct)
    {
        var connection = db.Database.GetDbConnection();
        var userId = currentUserAccessor.UserId ?? Guid.Empty;

        const string sql = """
                           SELECT
                               COUNT(DISTINCT tl.Id) AS TaskListCount,
                               COUNT(ti.Id) AS TotalTaskCount,
                               COALESCE(SUM(CASE WHEN ti.Status = 'Todo' THEN 1 ELSE 0 END), 0) AS TodoCount,
                               COALESCE(SUM(CASE WHEN ti.Status = 'InProgress' THEN 1 ELSE 0 END), 0) AS InProgressCount,
                               COALESCE(SUM(CASE WHEN ti.Status = 'Done' THEN 1 ELSE 0 END), 0) AS DoneCount,
                               COALESCE(SUM(CASE WHEN ti.Status = 'Cancelled' THEN 1 ELSE 0 END), 0) AS CancelledCount
                           FROM task_lists tl
                           LEFT JOIN task_items ti ON ti.TaskListId = tl.Id
                           WHERE tl.UserId = @UserId;

                           SELECT TOP (@RecentActivityCount)
                               ta.Id,
                               ta.TaskListId,
                               ta.TaskItemId,
                               ta.Type,
                               ta.Message,
                               ta.CreatedAtUtc,
                               ta.TaskTitleSnapshot
                           FROM task_activities ta
                           INNER JOIN task_lists tl ON tl.Id = ta.TaskListId
                           WHERE tl.UserId = @UserId
                           ORDER BY ta.CreatedAtUtc DESC, ta.Id DESC;
                           """;

        var command = new CommandDefinition(
            sql,
            new { UserId = userId, RecentActivityCount = recentActivityCount },
            cancellationToken: ct);

        await using var gridReader = await connection.QueryMultipleAsync(command);
        var summary = await gridReader.ReadSingleAsync<DashboardSummaryReadModel>();
        var recentActivities = (await gridReader.ReadAsync<TaskActivityReadModel>()).ToList();

        return summary with
        {
            RecentActivities = recentActivities
        };
    }
}
