using Microsoft.EntityFrameworkCore;
using TaskHub.Infrastructure.Persistence;

namespace TaskHub.Api.Auth;

public sealed class AuthSchemaAvailabilityChecker(TaskHubDbContext dbContext) : IAuthSchemaAvailabilityChecker
{
    public const string AddUsersTableMigrationId = "20260406163148_AddUsersTable";

    public async Task<bool> IsUsersSchemaAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var appliedMigrations = await dbContext.Database.GetAppliedMigrationsAsync(cancellationToken);
            return appliedMigrations.Contains(AddUsersTableMigrationId, StringComparer.OrdinalIgnoreCase);
        }
        catch (Exception)
        {
            return false;
        }
    }
}
