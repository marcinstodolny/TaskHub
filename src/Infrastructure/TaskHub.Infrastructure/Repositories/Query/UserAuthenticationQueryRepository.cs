using Microsoft.EntityFrameworkCore;
using TaskHub.Application.Abstractions.Repositories.Query;
using TaskHub.Domain.Entities;
using TaskHub.Infrastructure.Persistence;

namespace TaskHub.Infrastructure.Repositories.Query;

internal sealed class UserAuthenticationQueryRepository(TaskHubDbContext dbContext) : IUserAuthenticationQueryRepository
{
    public Task<User?> FindByNormalizedUsernameAsync(string normalizedUsername, CancellationToken ct)
    {
        return dbContext.Users.SingleOrDefaultAsync(user => user.Username == normalizedUsername, ct);
    }
}
