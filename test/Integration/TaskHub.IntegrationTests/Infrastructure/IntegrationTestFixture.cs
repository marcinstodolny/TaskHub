using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using TaskHub.Api.Auth;
using TaskHub.Api.Auth.Options;
using TaskHub.Application.abstraction;
using TaskHub.Contracts.Auth;
using TaskHub.Domain.Entities;
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

    public async Task ResetAsync()
    {
        await Database.ResetDatabaseAsync();

        await using var scope = Services.CreateAsyncScope();
        var demoUserInitializer = scope.ServiceProvider.GetRequiredService<DemoUserInitializer>();
        await demoUserInitializer.EnsureDefaultUserAsync();
    }

    public Task<TResult> SendAsync<TResult>(IRequest<TResult> request, CancellationToken ct = default)
    {
        return SendAsUserAsync(request, TaskHubApiFactory.TestUsername, ct);
    }

    public async Task<TResult> SendAsUserAsync<TResult>(IRequest<TResult> request, string? userIdentifier, CancellationToken ct = default)
    {
        await using var scope = Services.CreateAsyncScope();
        var currentUserAccessor = scope.ServiceProvider.GetRequiredService<TestCurrentUserAccessor>();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var resolvedUserIdentifier = await ResolveOrCreateUserIdentifierAsync(scope.ServiceProvider, userIdentifier, ct);
        using var _ = currentUserAccessor.BeginScope(resolvedUserIdentifier);
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
        var user = await ResolveOrCreateUserAsync(scope.ServiceProvider, userIdentifier, ct);

        var token = tokenGenerator.Generate(user.Id, user.Username, user.Role);

        var client = ApiFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static async Task<string?> ResolveOrCreateUserIdentifierAsync(IServiceProvider serviceProvider, string? userIdentifier, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(userIdentifier))
        {
            return null;
        }

        var user = await ResolveOrCreateUserAsync(serviceProvider, userIdentifier, ct);
        return user.Id.ToString();
    }

    private static async Task<User> ResolveOrCreateUserAsync(IServiceProvider serviceProvider, string userIdentifier, CancellationToken ct)
    {
        var dbContext = serviceProvider.GetRequiredService<TaskHubDbContext>();
        var passwordHasher = serviceProvider.GetRequiredService<IUserPasswordHasher>();
        var demoAuthOptions = serviceProvider.GetRequiredService<IOptions<DemoAuthOptions>>();

        if (Guid.TryParse(userIdentifier, out var userId))
        {
            var existingUserById = await dbContext.Users.SingleOrDefaultAsync(x => x.Id == userId, ct);
            if (existingUserById is not null)
            {
                return existingUserById;
            }

            var createByIdResult = User.Create(
                userId,
                userIdentifier,
                passwordHasher.HashPassword(TaskHubApiFactory.TestPassword),
                userIdentifier,
                demoAuthOptions.Value.Role);

            await dbContext.Users.AddAsync(createByIdResult.Value, ct);
            await dbContext.SaveChangesAsync(ct);
            return createByIdResult.Value;
        }

        var existingUserByUsername = await dbContext.Users.SingleOrDefaultAsync(x => x.Username == userIdentifier, ct);
        if (existingUserByUsername is not null)
        {
            return existingUserByUsername;
        }

        var createResult = User.Create(
            Guid.NewGuid(),
            userIdentifier,
            passwordHasher.HashPassword(TaskHubApiFactory.TestPassword),
            userIdentifier,
            demoAuthOptions.Value.Role);

        await dbContext.Users.AddAsync(createResult.Value, ct);
        await dbContext.SaveChangesAsync(ct);
        return createResult.Value;
    }
}
