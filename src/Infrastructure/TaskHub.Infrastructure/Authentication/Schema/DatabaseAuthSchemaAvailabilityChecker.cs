using Microsoft.EntityFrameworkCore;
using TaskHub.Application.Abstractions;
using TaskHub.Infrastructure.Persistence;

namespace TaskHub.Infrastructure.Authentication.Schema;

public sealed class DatabaseAuthSchemaAvailabilityChecker(TaskHubDbContext dbContext) : IAuthSchemaAvailabilityChecker
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
