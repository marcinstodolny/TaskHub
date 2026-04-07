using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using TaskHub.Application.Abstractions;
using TaskHub.Application.Exceptions;

namespace TaskHub.Infrastructure.Persistence
{
    public sealed class UnitOfWork(TaskHubDbContext db) : IUnitOfWork
    {
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await db.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException exception) when (IsDuplicateUsernameConflict(exception))
            {
                throw new DuplicateUsernameException();
            }
        }

        private static bool IsDuplicateUsernameConflict(DbUpdateException exception)
        {
            if (exception.InnerException is not SqlException sqlException)
            {
                return false;
            }

            return sqlException.Errors
                .Cast<SqlError>()
                .Any(error =>
                    (error.Number == 2601 || error.Number == 2627)
                    && error.Message.Contains("IX_users_Username", StringComparison.OrdinalIgnoreCase));
        }
    }
}
