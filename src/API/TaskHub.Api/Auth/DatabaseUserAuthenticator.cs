using Microsoft.EntityFrameworkCore;
using TaskHub.Application.abstraction;
using TaskHub.Domain.Entities;
using TaskHub.Infrastructure.Persistence;

namespace TaskHub.Api.Auth;

public sealed class DatabaseUserAuthenticator(TaskHubDbContext dbContext, IUserPasswordHasher passwordHasher)
{
    public async Task<User?> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .SingleOrDefaultAsync(x => x.Username == username, cancellationToken);

        if (user is null)
        {
            return null;
        }

        return passwordHasher.VerifyPassword(user.PasswordHash, password)
            ? user
            : null;
    }
}
