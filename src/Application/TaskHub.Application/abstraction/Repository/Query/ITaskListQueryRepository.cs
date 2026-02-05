using TaskHub.Application.Features.TaskList.Response;
using TaskHub.Application.Response;

namespace TaskHub.Application.abstraction.Repository.Query
{
    public interface ITaskListQueryRepository
    {
        Task<PaginatedResponse<TaskListResponse>> GetAllAsync(int page, int count, CancellationToken ct);
        Task<TaskListResponse?> GetByIdAsync(Guid listId, CancellationToken ct);
    }
}
