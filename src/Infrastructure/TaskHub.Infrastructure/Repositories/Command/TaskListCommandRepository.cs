using Microsoft.EntityFrameworkCore;
using TaskHub.Application.abstraction;
using TaskHub.Application.abstraction.Repository.Command;
using TaskHub.Domain.Base;
using TaskHub.Domain.Entities;
using TaskHub.Infrastructure.Persistence;

namespace TaskHub.Infrastructure.Repositories.Command
{
    internal sealed class TaskListCommandRepository(TaskHubDbContext db, ICurrentUserAccessor currentUserAccessor) : ITaskListCommandRepository
    {
        public async Task AddAsync(TaskList task, CancellationToken ct)
        {
            await db.TaskLists.AddAsync(task, ct);
        }

        public void Remove(TaskList task)
        {
            db.TaskLists.Remove(task);
        }

        public async Task<Result<TaskList>> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var ownerIdentifier = currentUserAccessor.UserIdentifier ?? string.Empty;
            var task = await db.TaskLists.FirstOrDefaultAsync(
                t => t.Id == id && t.OwnerIdentifier == ownerIdentifier,
                ct);
            return task is null ? Result.Fail<TaskList>($"Task list with id {id} not found") : Result.Success(task);
        }
    }
}
