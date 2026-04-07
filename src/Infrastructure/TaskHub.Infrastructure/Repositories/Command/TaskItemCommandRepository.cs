using Microsoft.EntityFrameworkCore;
using TaskHub.Application.Abstractions;
using TaskHub.Application.Abstractions.Repositories.Command;
using TaskHub.Domain.Base;
using TaskHub.Domain.Entities;
using TaskHub.Infrastructure.Persistence;

namespace TaskHub.Infrastructure.Repositories.Command
{
    internal sealed class TaskItemCommandRepository(TaskHubDbContext db, ICurrentUserAccessor currentUserAccessor) : ITaskItemCommandRepository
    {
        public async Task AddAsync(TaskItem task, CancellationToken ct)
        {
            await db.TaskItems.AddAsync(task, ct);
        }

        public void Remove(TaskItem task)
        {
            db.TaskItems.Remove(task);
        }

        public async Task<Result<TaskItem>> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var userId = currentUserAccessor.UserId ?? Guid.Empty;
            var task = await db.TaskItems
                .Join(
                    db.TaskLists.Where(list => list.UserId == userId),
                    item => item.TaskListId,
                    list => list.Id,
                    (item, _) => item)
                .FirstOrDefaultAsync(item => item.Id == id, ct);
            return task is null ? Result.Fail<TaskItem>($"Task with id {id} not found") : Result.Success(task);
        }
    }
}
