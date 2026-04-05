using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using TaskHub.Api.Auth;
using TaskHub.Api.Auth.Options;
using TaskHub.Contracts.Auth;
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

    public async Task<HttpClient> CreateAuthorizedClientAsync(CancellationToken ct = default)
    {
        using var authClient = ApiFactory.CreateClient();

        using var tokenResponse = await authClient.PostAsJsonAsync("/api/auth/token", new TokenRequest(
            TaskHubApiFactory.TestUsername,
            TaskHubApiFactory.TestPassword), ct);

        tokenResponse.EnsureSuccessStatusCode();

        var payload = await tokenResponse.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: ct);
        Assert.NotNull(payload);

        var client = ApiFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", payload!.AccessToken);
        return client;
    }

    public async Task<HttpClient> CreateAuthorizedClientAsync(string userIdentifier, CancellationToken ct = default)
    {
        await using var scope = Services.CreateAsyncScope();
        var tokenGenerator = scope.ServiceProvider.GetRequiredService<JwtTokenGenerator>();
        var demoAuthOptions = scope.ServiceProvider.GetRequiredService<IOptions<DemoAuthOptions>>();

        var token = tokenGenerator.Generate(userIdentifier, demoAuthOptions.Value.Role);

        var client = ApiFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
