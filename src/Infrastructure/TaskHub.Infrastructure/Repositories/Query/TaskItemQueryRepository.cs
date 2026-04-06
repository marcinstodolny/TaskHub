using Dapper;
using Microsoft.EntityFrameworkCore;
using System.Data;
using TaskHub.Application.abstraction;
using TaskHub.Application.abstraction.Repository.Query;
using TaskHub.Application.Common.Pagination;
using TaskHub.Application.Features.TaskItem.ReadModels;
using TaskHub.Infrastructure.Persistence;

namespace TaskHub.Infrastructure.Repositories.Query
{
    internal sealed class TaskItemQueryRepository(TaskHubDbContext db, ICurrentUserAccessor currentUserAccessor) : ITaskItemQueryRepository
    {
        public async Task<TaskItemReadModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var connection = db.Database.GetDbConnection();
            var ownerIdentifier = currentUserAccessor.UserIdentifier ?? string.Empty;
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
                               INNER JOIN task_lists tl ON tl.Id = ti.TaskListId
                               WHERE ti.Id = @Id
                                 AND tl.OwnerIdentifier = @OwnerIdentifier;
                               """;


            var command = new CommandDefinition(
                sql,
                new { Id = id, OwnerIdentifier = ownerIdentifier },
                cancellationToken: ct);
            return await connection.QuerySingleOrDefaultAsync<TaskItemReadModel>(command);
        }

        public async Task<PagedResult<TaskItemReadModel>> GetAllAsync(int page, int count, CancellationToken ct = default)
        {
            var connection = db.Database.GetDbConnection();
            var ownerIdentifier = currentUserAccessor.UserIdentifier ?? string.Empty;

            const string sql = """
                               SELECT COUNT(*)
                               FROM task_items ti
                               INNER JOIN task_lists tl ON tl.Id = ti.TaskListId
                               WHERE tl.OwnerIdentifier = @OwnerIdentifier

                               SELECT
                                   ti.Id,
                                   ti.TaskListId,
                                   ti.Title,
                                   ti.Description,
                                   ti.Priority,
                                   ti.Status
                               FROM task_items ti
                               INNER JOIN task_lists tl ON tl.Id = ti.TaskListId
                               WHERE tl.OwnerIdentifier = @OwnerIdentifier
                               ORDER BY ti.Title
                               OFFSET @skip ROWS
                               FETCH NEXT @take ROWS ONLY;
                               """;

            var command = new CommandDefinition(
                sql,
                new
                {
                    OwnerIdentifier = ownerIdentifier,
                    skip = (page - 1) * count,
                    take = count
                },
                cancellationToken: ct);

            await using var gridReader = await connection.QueryMultipleAsync(command);
            var totalItemCount = await gridReader.ReadFirstAsync<int>();
            var items = (await gridReader.ReadAsync<TaskItemReadModel>()).ToList();

            return new PagedResult<TaskItemReadModel>
            {
                PageNumber = page,
                TotalPages = (int)Math.Ceiling(totalItemCount / (double)count),
                Items = items
            };
        }

        public async Task<IReadOnlyCollection<TaskItemReadModel>> GetByTaskListIdAsync(Guid taskListId, CancellationToken ct = default)
        {
            var connection = db.Database.GetDbConnection();
            var ownerIdentifier = currentUserAccessor.UserIdentifier ?? string.Empty;
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
                               INNER JOIN task_lists tl ON tl.Id = ti.TaskListId
                               WHERE ti.TaskListId = @TaskListId
                                 AND tl.OwnerIdentifier = @OwnerIdentifier
                               ORDER BY ti.Title;
                               """;

            var command = new CommandDefinition(
                sql,
                new { TaskListId = taskListId, OwnerIdentifier = ownerIdentifier },
                cancellationToken: ct);
            var items = await connection.QueryAsync<TaskItemReadModel>(command);
            return items.ToList();
        }
    }
}
