using FluentResults;
using TaskHub.Domain.Entities;

namespace TaskHub.Application.abstraction.Repository.Command
{
    public interface ITaskListCommandRepository
    {
        Task AddAsync(TaskList list, CancellationToken ct);
        void Remove(TaskList task);
        Task<Result<TaskList>> GetByIdAsync(Guid id, CancellationToken ct);
    }
}
