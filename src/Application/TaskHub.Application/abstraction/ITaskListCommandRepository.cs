using FluentResults;
using TaskHub.Domain.Entities;

namespace TaskHub.Application.abstraction
{
    public interface ITaskListCommandRepository
    {
        Task AddAsync(TaskList list, CancellationToken ct);
        Task<Result<TaskList>> GetByIdAsync(Guid id, CancellationToken ct);
    }
}
