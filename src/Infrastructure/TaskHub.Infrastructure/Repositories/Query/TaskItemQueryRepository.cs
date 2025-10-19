using Microsoft.EntityFrameworkCore;
using TaskHub.Application.abstraction.Repository.Query;
using TaskHub.Domain.Entities;
using TaskHub.Infrastructure.Persistence;

namespace TaskHub.Infrastructure.Repositories.Query
{
    internal sealed class TaskItemQueryRepository(TaskHubDbContext db) : ITaskItemQueryRepository
    {
        public async Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await db.Set<TaskItem>().FirstOrDefaultAsync(t => t.Id == id, ct);
        }
    }
}
