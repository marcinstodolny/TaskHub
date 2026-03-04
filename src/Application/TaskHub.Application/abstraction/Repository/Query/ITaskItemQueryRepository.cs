using TaskHub.Application.Base.Response;
using TaskHub.Application.Features.TaskItem.Response;

namespace TaskHub.Application.abstraction.Repository.Query
{
    public interface ITaskItemQueryRepository
    {
        Task<TaskItemResponse?> GetByIdAsync(Guid taskId, CancellationToken ct);
        Task<PaginatedResponse<TaskItemResponse>> GetAllAsync(int page, int count, CancellationToken ct);
        Task<IReadOnlyCollection<TaskItemResponse>> GetByTaskListIdAsync(Guid taskListId, CancellationToken ct);
    }
}
