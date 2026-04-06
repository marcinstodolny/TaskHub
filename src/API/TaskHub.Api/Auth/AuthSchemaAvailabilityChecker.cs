using Microsoft.EntityFrameworkCore;
using TaskHub.Infrastructure.Persistence;

namespace TaskHub.Api.Auth;

public sealed class AuthSchemaAvailabilityChecker(TaskHubDbContext dbContext) : IAuthSchemaAvailabilityChecker
{
    public async Task<bool> IsUsersSchemaAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await dbContext.Users
                .AsNoTracking()
                .Select(static user => user.Id)
                .Take(1)
                .ToListAsync(cancellationToken);

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
