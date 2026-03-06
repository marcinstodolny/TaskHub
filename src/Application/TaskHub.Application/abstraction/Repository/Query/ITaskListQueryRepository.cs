using TaskHub.Application.Common.Pagination;
using TaskHub.Application.Features.TaskList.ReadModels;

namespace TaskHub.Application.abstraction.Repository.Query
{
    public interface ITaskListQueryRepository
    {
        Task<PagedResult<TaskListReadModel>> GetAllAsync(int page, int count, CancellationToken ct);
        Task<TaskListReadModel?> GetByIdAsync(Guid listId, CancellationToken ct);
    }
}
