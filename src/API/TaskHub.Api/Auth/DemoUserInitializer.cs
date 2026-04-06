using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TaskHub.Api.Auth.Options;
using TaskHub.Application.abstraction;
using TaskHub.Application.Features.Auth;
using TaskHub.Domain.Entities;
using TaskHub.Infrastructure.Persistence;

namespace TaskHub.Api.Auth;

public sealed class DemoUserInitializer(
    TaskHubDbContext dbContext,
    IOptions<DemoAuthOptions> demoAuthOptions,
    IUserPasswordHasher passwordHasher,
    ILogger<DemoUserInitializer> logger)
{
    public async Task EnsureDemoUserAsync(CancellationToken cancellationToken = default)
    {
        var options = demoAuthOptions.Value;
        if (!options.Enabled)
        {
            return;
        }

        var canonicalUsername = UsernameNormalizer.Normalize(options.Username);
        var fallbackDisplayName = UsernameNormalizer.TrimForDisplay(options.Username);

        var user = await dbContext.Users
            .SingleOrDefaultAsync(x => x.Id == options.UserId, cancellationToken);

        user ??= await dbContext.Users
            .SingleOrDefaultAsync(x => x.Username == canonicalUsername, cancellationToken);

        var displayName = string.IsNullOrWhiteSpace(options.DisplayName)
            ? fallbackDisplayName
            : options.DisplayName.Trim();

        if (user is null)
        {
            var passwordHash = passwordHasher.HashPassword(options.Password);
            var createResult = User.Create(options.UserId, canonicalUsername, passwordHash, displayName, options.Role);
            if (createResult.IsFailed)
            {
                throw new InvalidOperationException(string.Join("; ", createResult.Errors));
            }

            await dbContext.Users.AddAsync(createResult.Value, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Seeded demo user '{Username}' with id '{UserId}'.", canonicalUsername, options.UserId);
            return;
        }

        var passwordMatches = passwordHasher.VerifyPassword(user.PasswordHash, options.Password);
        var requiresUpdate =
            !string.Equals(user.Username, canonicalUsername, StringComparison.Ordinal) ||
            !string.Equals(user.DisplayName, displayName, StringComparison.Ordinal) ||
            !string.Equals(user.Role, options.Role, StringComparison.Ordinal) ||
            !passwordMatches;

        if (requiresUpdate)
        {
            var passwordHash = passwordMatches
                ? user.PasswordHash
                : passwordHasher.HashPassword(options.Password);

            user.Synchronize(canonicalUsername, passwordHash, displayName, options.Role);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        if (user.Id != options.UserId)
        {
            logger.LogWarning(
                "Demo user '{Username}' already exists with id '{ExistingUserId}' instead of configured id '{ConfiguredUserId}'. Existing id was preserved.",
                user.Username,
                user.Id,
                options.UserId);
        }
    }
}

