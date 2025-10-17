using TaskHub.Domain.Entities;

namespace TaskHub.Application.abstraction.Repository.Query
{
    public interface ITaskItemQueryRepository
    {
        Task<TaskItem?> GetByIdAsync(Guid taskId, CancellationToken ct); //TODO responses
    }
}
