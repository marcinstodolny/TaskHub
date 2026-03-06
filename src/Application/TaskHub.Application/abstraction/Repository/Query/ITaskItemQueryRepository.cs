using TaskHub.Application.Common.Pagination;
using TaskHub.Application.Features.TaskItem.ReadModels;

namespace TaskHub.Application.abstraction.Repository.Query
{
    public interface ITaskItemQueryRepository
    {
        Task<TaskItemReadModel?> GetByIdAsync(Guid taskId, CancellationToken ct);
        Task<PagedResult<TaskItemReadModel>> GetAllAsync(int page, int count, CancellationToken ct);
        Task<IReadOnlyCollection<TaskItemReadModel>> GetByTaskListIdAsync(Guid taskListId, CancellationToken ct);
    }
}
