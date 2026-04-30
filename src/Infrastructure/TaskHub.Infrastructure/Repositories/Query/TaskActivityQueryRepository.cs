using Dapper;
using Microsoft.EntityFrameworkCore;
using TaskHub.Application.Abstractions;
using TaskHub.Application.Abstractions.Repositories.Query;
using TaskHub.Application.Common.Pagination;
using TaskHub.Application.Features.TaskActivity.ReadModels;
using TaskHub.Infrastructure.Persistence;

namespace TaskHub.Infrastructure.Repositories.Query
{
    internal sealed class TaskActivityQueryRepository(TaskHubDbContext db, ICurrentUserAccessor currentUserAccessor) : ITaskActivityQueryRepository
    {
        public async Task<PagedResult<TaskActivityReadModel>> GetByTaskListIdAsync(Guid taskListId, int page, int count, CancellationToken ct)
        {
            var connection = db.Database.GetDbConnection();
            var userId = currentUserAccessor.UserId ?? Guid.Empty;

            const string sql = """
                               SELECT COUNT(*)
                               FROM task_activities ta
                               INNER JOIN task_lists tl ON tl.Id = ta.TaskListId
                               WHERE ta.TaskListId = @TaskListId
                                 AND tl.UserId = @UserId;

                               SELECT
                                   ta.Id,
                                   ta.TaskListId,
                                   ta.TaskItemId,
                                   ta.Type,
                                   ta.Message,
                                   ta.CreatedAtUtc,
                                   ta.TaskTitleSnapshot
                               FROM task_activities ta
                               INNER JOIN task_lists tl ON tl.Id = ta.TaskListId
                               WHERE ta.TaskListId = @TaskListId
                                 AND tl.UserId = @UserId
                               ORDER BY ta.CreatedAtUtc DESC, ta.Id DESC
                               OFFSET @skip ROWS
                               FETCH NEXT @take ROWS ONLY;
                               """;

            var command = new CommandDefinition(
                sql,
                new
                {
                    TaskListId = taskListId,
                    UserId = userId,
                    skip = (page - 1) * count,
                    take = count
                },
                cancellationToken: ct);

            await using var gridReader = await connection.QueryMultipleAsync(command);
            var totalItemCount = await gridReader.ReadFirstAsync<int>();
            var activities = (await gridReader.ReadAsync<TaskActivityReadModel>()).ToList();

            return new PagedResult<TaskActivityReadModel>
            {
                PageNumber = page,
                TotalPages = (int)Math.Ceiling(totalItemCount / (double)count),
                Items = activities
            };
        }
    }
}
