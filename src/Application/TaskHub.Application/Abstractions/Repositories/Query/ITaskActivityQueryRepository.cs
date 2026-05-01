using TaskHub.Application.Common.Pagination;
using TaskHub.Application.Features.TaskActivity.ReadModels;

namespace TaskHub.Application.Abstractions.Repositories.Query
{
    public interface ITaskActivityQueryRepository
    {
        Task<PagedResult<TaskActivityReadModel>> GetByTaskListIdAsync(Guid taskListId, int page, int count, CancellationToken ct);
    }
}
