using FluentResults;
using Microsoft.EntityFrameworkCore;
using TaskHub.Application.abstraction.Repository.Command;
using TaskHub.Domain.Entities;
using TaskHub.Infrastructure.Persistence;

namespace TaskHub.Infrastructure.Repositories.Command
{
    internal sealed class TaskItemCommandRepository(TaskHubDbContext db) : ITaskItemCommandRepository
    {
        public async Task AddAsync(TaskItem task, CancellationToken ct)
        {
            await db.AddAsync(task, ct);
        }

        public void Remove(TaskItem task)
        {
            db.Remove(task);
        }

        public async Task<Result<TaskItem>> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var task = await db.Set<TaskItem>().FirstOrDefaultAsync(t => t.Id == id, ct);
            return task is null ? Result.Fail($"Task with id {id} not found") : task;
        }
    }
}
