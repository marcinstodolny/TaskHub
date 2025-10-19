using TaskHub.Application.abstraction;

namespace TaskHub.Infrastructure.Persistence
{
    public sealed class UnitOfWork(TaskHubDbContext db) : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return db.SaveChangesAsync(cancellationToken);
        }
    }
}
