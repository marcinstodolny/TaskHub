using TaskHub.Application.Abstractions.Repositories.Command;
using TaskHub.Domain.Entities;
using TaskHub.Infrastructure.Persistence;

namespace TaskHub.Infrastructure.Repositories.Command
{
    internal sealed class TaskActivityCommandRepository(TaskHubDbContext db) : ITaskActivityCommandRepository
    {
        public async Task AddAsync(TaskActivity activity, CancellationToken ct)
        {
            await db.TaskActivities.AddAsync(activity, ct);
        }
    }
}
