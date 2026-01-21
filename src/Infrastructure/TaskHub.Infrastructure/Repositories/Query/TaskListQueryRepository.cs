using Dapper;
using Microsoft.EntityFrameworkCore;
using TaskHub.Application.abstraction.Repository.Query;
using TaskHub.Application.Response;
using TaskHub.Infrastructure.Persistence;

namespace TaskHub.Infrastructure.Repositories.Query
{
    internal sealed class TaskListQueryRepository(TaskHubDbContext db) : ITaskListQueryRepository
    {
        public async Task<List<TaskListResponse>> GetAllAsync(CancellationToken ct = default)
        {
            var connection = db.Database.GetDbConnection();

            const string sql = """
                               SELECT 
                                   tl.Id,
                                   tl.Title
                               FROM task_lists tl
                               ORDER BY tl.Title;

                               SELECT
                                   ti.Id,
                                   ti.TaskListId,
                                   ti.Title,
                                   ti.Description,
                                   ti.Priority,
                                   ti.Status
                               FROM task_items ti
                               ORDER BY ti.TaskListId, ti.Id;
                               """;

            var command = new CommandDefinition(sql, cancellationToken: ct);
            await using var gridReader = await connection.QueryMultipleAsync(command);

            var taskLists = (await gridReader.ReadAsync<TaskListResponse>()).ToList();
            var taskItems = (await gridReader.ReadAsync<TaskItemResponse>()).ToList();

            var taskListsById = taskLists.ToDictionary(list => list.Id);

            foreach (var taskItem in taskItems)
            {
                if (taskListsById.TryGetValue(taskItem.TaskListId, out var taskList))
                    taskList.Tasks.Add(taskItem);
            }

            return taskLists;
        }

        public async Task<TaskListResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var connection = db.Database.GetDbConnection();

            const string sql = """
                               SELECT TOP (1)
                                   tl.Id,
                                   tl.Title
                               FROM task_lists tl
                               WHERE tl.Id = @Id
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

            var taskList = await gridReader.ReadSingleOrDefaultAsync<TaskListResponse>();
            if (taskList is null)
                return null;

            var taskItems = (await gridReader.ReadAsync<TaskItemResponse>()).ToList();
            taskList.Tasks.AddRange(taskItems);

            return taskList;
        }

        public async Task<TaskListResponse?> GetByTitleAsync(string title, CancellationToken ct = default)
        {
            var connection = db.Database.GetDbConnection();

            const string sql = """
                               SELECT TOP (1)
                                   tl.Id,
                                   tl.Title
                               FROM task_lists tl
                               WHERE tl.Title = @Title
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
                                   WHERE tl.Title = @Title
                                   ORDER BY tl.Id
                               )
                               ORDER BY ti.Id;
                               """;

            var command = new CommandDefinition(sql, new { Title = title }, cancellationToken: ct);

            await using var gridReader = await connection.QueryMultipleAsync(command);

            var taskList = await gridReader.ReadSingleOrDefaultAsync<TaskListResponse>();
            if (taskList is null)
                return null;

            var taskItems = (await gridReader.ReadAsync<TaskItemResponse>()).ToList();
            taskList.Tasks.AddRange(taskItems);

            return taskList;
        }
    }
}