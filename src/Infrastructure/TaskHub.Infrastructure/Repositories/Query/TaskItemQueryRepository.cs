using Dapper;
using Microsoft.EntityFrameworkCore;
using System.Data;
using TaskHub.Application.abstraction.Repository.Query;
using TaskHub.Application.Base.Response;
using TaskHub.Application.Features.TaskItem.Response;
using TaskHub.Infrastructure.Persistence;

namespace TaskHub.Infrastructure.Repositories.Query
{
    internal sealed class TaskItemQueryRepository(TaskHubDbContext db) : ITaskItemQueryRepository
    {
        public async Task<TaskItemResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var connection = db.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync(ct);
            }

            const string sql = """
                               SELECT 
                                   Id,
                                   TaskListId,
                                   Title,
                                   Description,
                                   Priority,
                                   Status
                               FROM task_items
                               WHERE Id = @Id;
                               """;


            var command = new CommandDefinition(sql, new { Id = id }, cancellationToken: ct);
            return await connection.QuerySingleOrDefaultAsync<TaskItemResponse>(command);
        }

        public async Task<PaginatedResponse<TaskItemResponse>> GetAllAsync(int page, int count, CancellationToken ct = default)
        {
            var connection = db.Database.GetDbConnection();

            const string sql = """
                               SELECT COUNT(*)
                               FROM task_items ti

                               SELECT
                                   ti.Id,
                                   ti.TaskListId,
                                   ti.Title,
                                   ti.Description,
                                   ti.Priority,
                                   ti.Status
                               FROM task_items ti
                               ORDER BY ti.Title
                               OFFSET @skip ROWS
                               FETCH NEXT @take ROWS ONLY;
                               """;

            var command = new CommandDefinition(
                sql,
                new
                {
                    skip = (page - 1) * count,
                    take = count
                },
                cancellationToken: ct);

            await using var gridReader = await connection.QueryMultipleAsync(command);
            var totalItemCount = await gridReader.ReadFirstAsync<int>();
            var items = (await gridReader.ReadAsync<TaskItemResponse>()).ToList();

            return new PaginatedResponse<TaskItemResponse>
            {
                CurrentPage = page,
                TotalPageCount = (int)Math.Ceiling(totalItemCount / (double)count),
                Items = items
            };
        }

        public async Task<IReadOnlyCollection<TaskItemResponse>> GetByTaskListIdAsync(Guid taskListId, CancellationToken ct = default)
        {
            var connection = db.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync(ct);
            }

            const string sql = """
                               SELECT
                                   ti.Id,
                                   ti.TaskListId,
                                   ti.Title,
                                   ti.Description,
                                   ti.Priority,
                                   ti.Status
                               FROM task_items ti
                               WHERE ti.TaskListId = @TaskListId
                               ORDER BY ti.Title;
                               """;

            var command = new CommandDefinition(sql, new { TaskListId = taskListId }, cancellationToken: ct);
            var items = await connection.QueryAsync<TaskItemResponse>(command);
            return items.ToList();
        }
    }
}
