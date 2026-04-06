using Microsoft.EntityFrameworkCore;
using TaskHub.Application.abstraction.Repository.Command;
using TaskHub.Domain.Entities;
using TaskHub.Infrastructure.Persistence;

namespace TaskHub.Infrastructure.Repositories.Command;

internal sealed class UserCommandRepository(TaskHubDbContext db) : IUserCommandRepository
{
    public Task<bool> ExistsByUsernameAsync(string username, CancellationToken ct)
    {
        return db.Users.AnyAsync(x => x.Username == username, ct);
    }

    public async Task AddAsync(User user, CancellationToken ct)
    {
        await db.Users.AddAsync(user, ct);
    }
}
