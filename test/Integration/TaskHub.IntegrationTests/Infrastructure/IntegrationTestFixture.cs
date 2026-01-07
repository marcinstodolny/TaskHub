using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskHub.Infrastructure.Persistence;
using Xunit;

namespace TaskHub.IntegrationTests.Infrastructure;

[CollectionDefinition(nameof(IntegrationTestCollection))]
public class IntegrationTestCollection : ICollectionFixture<IntegrationTestFixture>;

public sealed class IntegrationTestFixture : IAsyncLifetime
{
    public TaskHubMsSqlFixture Database { get; } = new();

    public TaskHubApiFactory ApiFactory { get; private set; } = null!;

    public IServiceProvider Services => ApiFactory.Services;

    public async Task InitializeAsync()
    {
        await Database.InitializeAsync();

        Environment.SetEnvironmentVariable("ConnectionStrings__TaskHub", Database.ConnectionString);

        ApiFactory = new TaskHubApiFactory(Database);

        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TaskHubDbContext>();
        await dbContext.Database.MigrateAsync();

        await Database.InitializeRespawnerAsync();
    }

    public async Task DisposeAsync()
    {
        Environment.SetEnvironmentVariable("ConnectionStrings__TaskHub", null);

        await ApiFactory.DisposeAsync();
        await Database.DisposeAsync();
    }

    public async Task ResetAsync() => await Database.ResetDatabaseAsync();

    public async Task<TResult> SendAsync<TResult>(IRequest<TResult> request, CancellationToken ct = default)
    {
        await using var scope = Services.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        return await sender.Send(request, ct);
    }
}