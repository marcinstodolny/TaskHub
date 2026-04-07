using TaskHub.Domain.Base;
using TaskHub.Domain.Entities;

namespace TaskHub.Application.Abstractions.Repositories.Command
{
    public interface ITaskItemCommandRepository
    {
        Task AddAsync(TaskItem task, CancellationToken ct);
        void Remove(TaskItem task);
        Task<Result<TaskItem>> GetByIdAsync(Guid id, CancellationToken ct);
    }
}
