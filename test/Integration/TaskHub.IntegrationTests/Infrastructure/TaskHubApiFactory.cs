using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TaskHub.Api;
using TaskHub.Infrastructure.Persistence;

namespace TaskHub.IntegrationTests.Infrastructure;

public class TaskHubApiFactory(TaskHubMsSqlFixture database) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSolutionRelativeContentRoot("src/API/TaskHub.Api");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<TaskHubDbContext>>();

            services.AddDbContext<TaskHubDbContext>(options =>
                options.UseSqlServer(database.ConnectionString));
        });
    }
}