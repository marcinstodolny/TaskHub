using TaskHub.Domain.Entities;

namespace TaskHub.Application.abstraction.Repository.Query
{
    public interface ITaskListQueryRepository
    {
        Task<TaskList?> GetByIdAsync(Guid listId, CancellationToken ct);
    }
}
