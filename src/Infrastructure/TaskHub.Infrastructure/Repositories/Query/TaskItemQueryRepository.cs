using Dapper;
using Microsoft.EntityFrameworkCore;
using System.Data;
using TaskHub.Application.abstraction.Repository.Query;
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
    }
}
