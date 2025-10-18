using Microsoft.EntityFrameworkCore;
using TaskHub.Application.abstraction.Repository.Query;
using TaskHub.Domain.Entities;
using TaskHub.Infrastructure.Persistence;

namespace TaskHub.Infrastructure.Repositories.Query
{
    internal sealed class TaskListQueryRepository(TaskHubDbContext db) : ITaskListQueryRepository
    {
        public async Task<TaskList?> GetByIdAsync(Guid id, CancellationToken ct = default) //TODO Dapper
        {
            return await db.TaskLists.FirstOrDefaultAsync(t => t.Id == id, ct);
        }

        public async Task<TaskList?> GetByTitleAsync(string title, CancellationToken ct = default) //TODO Dapper
        {
            return await db.TaskLists.FirstOrDefaultAsync(t => t.Title.Value == title, ct);
        }
    }
}