using TaskHub.Application.Common.Pagination;
using TaskHub.Application.Features.TaskList.ReadModels;

namespace TaskHub.Application.Abstractions.Repositories.Query
{
    public interface ITaskListQueryRepository
    {
        Task<PagedResult<TaskListSummaryReadModel>> GetAllAsync(int page, int count, CancellationToken ct);
        Task<TaskListDetailsReadModel?> GetByIdAsync(Guid listId, CancellationToken ct);
    }
}
