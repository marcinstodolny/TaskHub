using TaskHub.Application.Response;

namespace TaskHub.Application.abstraction.Repository.Query
{
    public interface ITaskItemQueryRepository
    {
        Task<TaskItemResponse?> GetByIdAsync(Guid taskId, CancellationToken ct);
    }
}
