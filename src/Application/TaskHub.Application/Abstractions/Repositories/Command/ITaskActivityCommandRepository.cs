using TaskHub.Domain.Entities;

namespace TaskHub.Application.Abstractions.Repositories.Command
{
    public interface ITaskActivityCommandRepository
    {
        Task AddAsync(TaskActivity activity, CancellationToken ct);
    }
}
