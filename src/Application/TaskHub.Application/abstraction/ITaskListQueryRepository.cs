using FluentResults;
using TaskHub.Domain.Entities;

namespace TaskHub.Application.abstraction
{
    public interface ITaskListQueryRepository
    {
        Task<TaskList?> GetTasksByListIdAsync(Guid listId, CancellationToken ct);
    }
}
