using TaskHub.Application.Response;
using TaskHub.Domain.Entities;

namespace TaskHub.Application.abstraction.Repository.Query
{
    public interface ITaskListQueryRepository
    {
        Task<List<TaskListResponse>> GetAllAsync(CancellationToken ct); //TODO pagination
        Task<TaskListResponse?> GetByIdAsync(Guid listId, CancellationToken ct);
        Task<TaskListResponse?> GetByTitleAsync(string title, CancellationToken ct);
    }
}
