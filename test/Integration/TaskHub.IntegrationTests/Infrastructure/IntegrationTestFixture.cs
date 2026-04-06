using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using TaskHub.Application.abstraction;
using TaskHub.Contracts.Auth;
using TaskHub.Domain.Entities;
using TaskHub.Infrastructure.Authentication.Bootstrap;
using TaskHub.Infrastructure.Authentication.Options;
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
        await demoUserInitializer.EnsureDemoUserAsync();
    }

    public Task<TResult> SendAsync<TResult>(IRequest<TResult> request, CancellationToken ct = default)
    {
        return SendAsUserAsync(request, TaskHubApiFactory.TestUsername, ct);
    }

    public async Task<TResult> SendAsUserAsync<TResult>(IRequest<TResult> request, Guid userId, CancellationToken ct = default)
    {
        await using var scope = Services.CreateAsyncScope();
        var currentUserAccessor = scope.ServiceProvider.GetRequiredService<TestCurrentUserAccessor>();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var resolvedUserId = await ResolveOrCreateUserIdAsync(scope.ServiceProvider, userId, ct);
        using var _ = currentUserAccessor.BeginScope(resolvedUserId);
        return await sender.Send(request, ct);
    }

    public async Task<TResult> SendAsUserAsync<TResult>(IRequest<TResult> request, string? username, CancellationToken ct = default)
    {
        await using var scope = Services.CreateAsyncScope();
        var currentUserAccessor = scope.ServiceProvider.GetRequiredService<TestCurrentUserAccessor>();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var resolvedUserId = await ResolveOrCreateUserIdAsync(scope.ServiceProvider, username, ct);
        using var _ = currentUserAccessor.BeginScope(resolvedUserId);
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

    public async Task<HttpClient> CreateAuthorizedClientAsync(string usernameOrUserId, CancellationToken ct = default)
    {
        await using var scope = Services.CreateAsyncScope();
        var user = await ResolveOrCreateUserAsync(scope.ServiceProvider, usernameOrUserId, ct);
        return CreateAuthorizedClient(user, scope.ServiceProvider);
    }

    public Task<HttpClient> CreateAuthorizedClientAsync(User user, CancellationToken ct = default)
    {
        return Task.FromResult(CreateAuthorizedClient(user, Services));
    }

    public async Task<User> EnsureUserAsync(Guid userId, string username, CancellationToken ct = default)
    {
        await using var scope = Services.CreateAsyncScope();
        return await ResolveOrCreateUserAsync(scope.ServiceProvider, userId, username, ct);
    }

    private static async Task<Guid?> ResolveOrCreateUserIdAsync(IServiceProvider serviceProvider, string? username, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return null;
        }

        var user = await ResolveOrCreateUserAsync(serviceProvider, username, ct);
        return user.Id;
    }

    private static async Task<Guid> ResolveOrCreateUserIdAsync(IServiceProvider serviceProvider, Guid userId, CancellationToken ct)
    {
        var user = await ResolveOrCreateUserAsync(serviceProvider, userId, userId.ToString(), ct);
        return user.Id;
    }

    private static async Task<User> ResolveOrCreateUserAsync(IServiceProvider serviceProvider, string usernameOrUserId, CancellationToken ct)
    {
        if (Guid.TryParse(usernameOrUserId, out var userId))
        {
            return await ResolveOrCreateUserAsync(serviceProvider, userId, usernameOrUserId, ct);
        }

        var dbContext = serviceProvider.GetRequiredService<TaskHubDbContext>();
        var existingUserByUsername = await dbContext.Users.SingleOrDefaultAsync(x => x.Username == usernameOrUserId, ct);
        if (existingUserByUsername is not null)
        {
            return existingUserByUsername;
        }

        return await ResolveOrCreateUserAsync(serviceProvider, Guid.NewGuid(), usernameOrUserId, ct);
    }

    private static async Task<User> ResolveOrCreateUserAsync(IServiceProvider serviceProvider, Guid userId, string username, CancellationToken ct)
    {
        var dbContext = serviceProvider.GetRequiredService<TaskHubDbContext>();
        var passwordHasher = serviceProvider.GetRequiredService<IUserPasswordHasher>();
        var demoAuthOptions = serviceProvider.GetRequiredService<IOptions<DemoAuthOptions>>();
        var passwordHash = passwordHasher.HashPassword(TaskHubApiFactory.TestPassword);

        var existingUserById = await dbContext.Users.SingleOrDefaultAsync(x => x.Id == userId, ct);
        if (existingUserById is not null)
        {
            if (!string.Equals(existingUserById.Username, username, StringComparison.Ordinal)
                || !string.Equals(existingUserById.DisplayName, username, StringComparison.Ordinal)
                || !string.Equals(existingUserById.Role, demoAuthOptions.Value.Role, StringComparison.Ordinal)
                || !passwordHasher.VerifyPassword(existingUserById.PasswordHash, TaskHubApiFactory.TestPassword))
            {
                existingUserById.Synchronize(username, passwordHash, username, demoAuthOptions.Value.Role);
                await dbContext.SaveChangesAsync(ct);
            }

            return existingUserById;
        }

        var existingUserByUsername = await dbContext.Users.SingleOrDefaultAsync(x => x.Username == username, ct);
        if (existingUserByUsername is not null)
        {
            if (existingUserByUsername.Id != userId)
            {
                throw new InvalidOperationException(
                    $"Test user '{username}' already exists with id '{existingUserByUsername.Id}' but expected '{userId}'.");
            }

            return existingUserByUsername;
        }

        var createResult = User.Create(
            userId,
            username,
            passwordHash,
            username,
            demoAuthOptions.Value.Role);

        await dbContext.Users.AddAsync(createResult.Value, ct);
        await dbContext.SaveChangesAsync(ct);
        return createResult.Value;
    }

    private HttpClient CreateAuthorizedClient(User user, IServiceProvider serviceProvider)
    {
        var tokenGenerator = serviceProvider.GetRequiredService<IAccessTokenGenerator>();
        var token = tokenGenerator.GenerateAccessToken(user.Id, user.Username, user.Role);

        var client = ApiFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
