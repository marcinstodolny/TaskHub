using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TaskHub.Infrastructure.Authentication.Bootstrap;
using TaskHub.Infrastructure.Persistence;

namespace TaskHub.Infrastructure.Hosting;

public static class WebApplicationExtensions
{
    private const string AddUsersTableMigrationId = "20260406163148_AddUsersTable";

    public static WebApplication ApplyInfrastructureInitialization(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("Testing"))
        {
            return app;
        }

        using var scope = app.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbMigrations");
        var dbContext = scope.ServiceProvider.GetRequiredService<TaskHubDbContext>();
        var migrator = dbContext.GetService<IMigrator>();
        var demoUserInitializer = scope.ServiceProvider.GetRequiredService<DemoUserInitializer>();
        var demoDataInitializer = scope.ServiceProvider.GetRequiredService<DemoDataInitializer>();

        const int maxAttempts = 10;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                logger.LogInformation("Applying EF Core migrations (attempt {Attempt}/{MaxAttempts})...", attempt, maxAttempts);

                var appliedMigrations = dbContext.Database.GetAppliedMigrations().ToHashSet(StringComparer.OrdinalIgnoreCase);
                var pendingMigrations = dbContext.Database.GetPendingMigrations().ToHashSet(StringComparer.OrdinalIgnoreCase);

                if (!appliedMigrations.Contains(AddUsersTableMigrationId)
                    && pendingMigrations.Contains(AddUsersTableMigrationId))
                {
                    migrator.Migrate(AddUsersTableMigrationId);
                    logger.LogInformation("Migration {MigrationId} applied.", AddUsersTableMigrationId);
                }

                if (dbContext.Database.GetAppliedMigrations().Contains(AddUsersTableMigrationId, StringComparer.OrdinalIgnoreCase))
                {
                    demoUserInitializer.EnsureDemoUserAsync().GetAwaiter().GetResult();
                }

                dbContext.Database.Migrate();
                demoDataInitializer.EnsureDemoDataAsync().GetAwaiter().GetResult();
                logger.LogInformation("Migrations applied.");
                break;
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception, "Migration attempt {Attempt} failed.", attempt);

                if (attempt == maxAttempts)
                {
                    throw;
                }

                Thread.Sleep(TimeSpan.FromSeconds(2));
            }
        }

        return app;
    }
}
