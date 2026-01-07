using Microsoft.EntityFrameworkCore;
using TaskHub.Application.abstraction.Repository.Query;
using TaskHub.Application.Response;
using TaskHub.Domain.Entities;
using TaskHub.Infrastructure.Persistence;

namespace TaskHub.Infrastructure.Repositories.Query
{
    internal sealed class TaskListQueryRepository(TaskHubDbContext db) : ITaskListQueryRepository
    {
        public async Task<List<TaskListResponse>> GetAllAsync(CancellationToken ct = default) //TODO Dapper
        {
            return (await db.Set<TaskList>().Include(taskList => taskList.Tasks).ToListAsync(cancellationToken: ct)).Select(t => new TaskListResponse
            {
                Title = t.Title.Value, Id = t.Id,
                Tasks = t.Tasks.Select(task => new TaskItemResponse { Id = task.Id, Title = task.Title.Value }).ToList()
            }).ToList();
        }

        public async Task<TaskListResponse?> GetByIdAsync(Guid id, CancellationToken ct = default) //TODO Dapper
        {
            var list = await db.Set<TaskList>().Include(taskList => taskList.Tasks).FirstOrDefaultAsync(t => t.Id == id, ct);
            return list is null ? null : new TaskListResponse 
            {
                Id = list.Id, Title = list.Title.Value,
                Tasks = list.Tasks.Select(task => new TaskItemResponse { Id = task.Id, Title = task.Title.Value })
                    .ToList()
            };
        }

        public async Task<TaskListResponse?> GetByTitleAsync(string title, CancellationToken ct = default) //TODO Dapper
        {
            var list = await db.Set<TaskList>().Include(taskList => taskList.Tasks).FirstOrDefaultAsync(t => t.Title.Value == title, ct);
            return list is null ? null : new TaskListResponse 
            {
                Id = list.Id, Title = list.Title.Value,
                Tasks = list.Tasks.Select(task => new TaskItemResponse{ Id = task.Id, Title = task.Title.Value }).ToList()
            };
        }
    }
}