using FluentResults;
using Microsoft.EntityFrameworkCore;
using TaskHub.Application.abstraction.Repository.Command;
using TaskHub.Domain.Entities;
using TaskHub.Infrastructure.Persistence;

namespace TaskHub.Infrastructure.Repositories.Command
{
    internal sealed class TaskListCommandRepository(TaskHubDbContext db) : ITaskListCommandRepository
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
            var task = await db.TaskLists.FirstOrDefaultAsync(t => t.Id == id, ct);
            return task is null ? Result.Fail($"Task list with id {id} not found") : task;
        }
    }
}
