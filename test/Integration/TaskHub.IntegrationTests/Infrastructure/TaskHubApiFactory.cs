using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TaskHub.Api;
using TaskHub.Infrastructure.Persistence;

namespace TaskHub.IntegrationTests.Infrastructure;

public class TaskHubApiFactory(TaskHubMsSqlFixture database) : WebApplicationFactory<Program>
{
    public const string TestUsername = "integration-user";
    public const string TestPassword = "integration-pass";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSolutionRelativeContentRoot("src/API/TaskHub.Api");

        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "TaskHub.Api.Tests",
                ["Jwt:Audience"] = "TaskHub.IntegrationTests",
                ["Jwt:SigningKey"] = "taskhub-integration-tests-signing-key-123",
                ["Jwt:ExpiresInMinutes"] = "60",
                ["DemoAuth:Enabled"] = "true",
                ["DemoAuth:Username"] = TestUsername,
                ["DemoAuth:Password"] = TestPassword,
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<TaskHubDbContext>>();

            services.AddDbContext<TaskHubDbContext>(options =>
                options.UseSqlServer(database.ConnectionString));
        });
    }
}
