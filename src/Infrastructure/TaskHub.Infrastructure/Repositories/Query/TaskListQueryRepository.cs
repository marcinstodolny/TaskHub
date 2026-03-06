using Dapper;
using Microsoft.EntityFrameworkCore;
using TaskHub.Application.abstraction.Repository.Query;
using TaskHub.Application.Common.Pagination;
using TaskHub.Application.Features.TaskItem.ReadModels;
using TaskHub.Application.Features.TaskList.ReadModels;
using TaskHub.Infrastructure.Persistence;

namespace TaskHub.Infrastructure.Repositories.Query
{
    internal sealed class TaskListQueryRepository(TaskHubDbContext db) : ITaskListQueryRepository
    {
        public async Task<PagedResult<TaskListSummaryReadModel>> GetAllAsync(int page, int count,
            CancellationToken ct = default)
        {
            var connection = db.Database.GetDbConnection();

            const string sql = """
                               SELECT COUNT(*)
                               FROM task_lists;

                               SELECT
                                   tl.Id,
                                   tl.Title,
                                   COUNT(ti.Id) AS TasksCount
                               FROM task_lists tl
                               LEFT JOIN task_items ti ON ti.TaskListId = tl.Id
                               GROUP BY tl.Id, tl.Title
                               ORDER BY tl.Title
                               OFFSET @skip ROWS
                               FETCH NEXT @take ROWS ONLY;
                               """;

            var command = new CommandDefinition(sql, new { skip = (page - 1) * count, take = count },
                cancellationToken: ct);
            await using var gridReader = await connection.QueryMultipleAsync(command);

            var totalItemCount = await gridReader.ReadFirstAsync<int>();
            var taskLists = (await gridReader.ReadAsync<TaskListSummaryReadModel>()).ToList();
            return new PagedResult<TaskListSummaryReadModel>
            {
                PageNumber = page,
                TotalPages = (int)Math.Ceiling(totalItemCount / (double)count),
                Items = taskLists
            };
        }

        public async Task<TaskListDetailsReadModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var connection = db.Database.GetDbConnection();

            const string sql = """
                               SELECT TOP (1)
                                   tl.Id,
                                   tl.Title,
                                   COUNT(ti.Id) AS TasksCount
                               FROM task_lists tl
                               LEFT JOIN task_items ti ON ti.TaskListId = tl.Id
                               WHERE tl.Id = @Id
                               GROUP BY tl.Id, tl.Title
                               ORDER BY tl.Id;

                               SELECT
                                   ti.Id,
                                   ti.TaskListId,
                                   ti.Title,
                                   ti.Description,
                                   ti.Priority,
                                   ti.Status
                               FROM task_items ti
                               WHERE ti.TaskListId = (
                                   SELECT TOP (1) tl.Id
                                   FROM task_lists tl
                                   WHERE tl.Id = @Id
                                   ORDER BY tl.Id
                               )
                               ORDER BY ti.Id;
                               """;

            var command = new CommandDefinition(sql, new { Id = id }, cancellationToken: ct);

            await using var gridReader = await connection.QueryMultipleAsync(command);

            var taskList = await gridReader.ReadSingleOrDefaultAsync<TaskListDetailsReadModel>();
            if (taskList is null)
                return null;

            var taskItems = (await gridReader.ReadAsync<TaskItemReadModel>()).ToList();
            taskList.Tasks.AddRange(taskItems);

            return taskList;
        }
    }
}
