using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using TaskHub.Contracts.Auth;

namespace TaskHub.Web.Auth;

public sealed class AuthSession(IHttpClientFactory httpClientFactory, ProtectedSessionStorage protectedSessionStorage)
{
    private const string StorageKey = "taskhub.auth-session";

    private readonly SemaphoreSlim _stateLock = new(1, 1);

    public bool IsInitialized { get; private set; }
    public bool IsAuthenticated => HasValidToken();
    public string? Username { get; private set; }
    public string? AccessToken { get; private set; }
    public DateTimeOffset? ExpiresAtUtc { get; private set; }

    public event Action? StateChanged;

    public async Task InitializeAsync(CancellationToken ct = default)
    {
        if (IsInitialized)
        {
            return;
        }

        await _stateLock.WaitAsync(ct);
        try
        {
            if (IsInitialized)
            {
                return;
            }

            var storedSession = await protectedSessionStorage.GetAsync<PersistedAuthSession>(StorageKey);
            if (storedSession.Success && storedSession.Value is not null)
            {
                ApplyState(storedSession.Value.Username, storedSession.Value.AccessToken, storedSession.Value.ExpiresAtUtc);

                if (!HasValidToken())
                {
                    ClearState(notify: false);
                    await protectedSessionStorage.DeleteAsync(StorageKey);
                }
            }

            IsInitialized = true;
            NotifyStateChanged();
        }
        finally
        {
            _stateLock.Release();
        }
    }

    public async Task<AuthLoginResult> LoginAsync(string username, string password, CancellationToken ct = default)
    {
        var normalizedUsername = username.Trim();

        if (string.IsNullOrWhiteSpace(normalizedUsername) || string.IsNullOrWhiteSpace(password))
        {
            return AuthLoginResult.Failure("Username and password are required.");
        }

        await _stateLock.WaitAsync(ct);
        try
        {
            var tokenRequest = new TokenRequest(normalizedUsername, password);
            var apiClient = httpClientFactory.CreateClient("TaskHubApi");
            using var response = await apiClient.PostAsJsonAsync("/api/auth/token", tokenRequest, ct);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await ClearStateAsync(notify: false);
                return AuthLoginResult.Failure("Invalid username or password.");
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                await ClearStateAsync(notify: false);
                return AuthLoginResult.Failure("Login is currently unavailable.");
            }

            if (!response.IsSuccessStatusCode)
            {
                await ClearStateAsync(notify: false);
                return AuthLoginResult.Failure("Unable to sign in right now. Please try again.");
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: ct);
            if (string.IsNullOrWhiteSpace(tokenResponse?.AccessToken))
            {
                await ClearStateAsync(notify: false);
                return AuthLoginResult.Failure("The API returned an invalid token.");
            }

            var expiresAtUtc = ResolveExpirationUtc(tokenResponse.AccessToken);
            if (expiresAtUtc is null)
            {
                await ClearStateAsync(notify: false);
                return AuthLoginResult.Failure("The API returned a token without a valid expiration.");
            }

            ApplyState(normalizedUsername, tokenResponse.AccessToken, expiresAtUtc.Value);
            await PersistStateAsync();
            IsInitialized = true;

            NotifyStateChanged();
            return AuthLoginResult.Success();
        }
        finally
        {
            _stateLock.Release();
        }
    }

    public string GetRequiredAccessToken()
    {
        if (!HasValidToken() || string.IsNullOrWhiteSpace(AccessToken))
        {
            throw new InvalidOperationException("You need to sign in before calling the API.");
        }

        return AccessToken;
    }

    public async Task LogoutAsync(CancellationToken ct = default)
    {
        await _stateLock.WaitAsync(ct);
        try
        {
            await ClearStateAsync();
        }
        finally
        {
            _stateLock.Release();
        }
    }

    private bool HasValidToken()
    {
        if (string.IsNullOrWhiteSpace(AccessToken) || ExpiresAtUtc is null)
        {
            return false;
        }

        return ExpiresAtUtc > DateTimeOffset.UtcNow.AddMinutes(1);
    }

    private void ApplyState(string username, string accessToken, DateTimeOffset expiresAtUtc)
    {
        Username = username;
        AccessToken = accessToken;
        ExpiresAtUtc = expiresAtUtc;
    }

    private async Task PersistStateAsync()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(AccessToken) || ExpiresAtUtc is null)
        {
            return;
        }

        var persistedSession = new PersistedAuthSession(Username, AccessToken, ExpiresAtUtc.Value);
        await protectedSessionStorage.SetAsync(StorageKey, persistedSession);
    }

    private async Task ClearStateAsync(bool notify = true)
    {
        ClearState(notify: false);
        await protectedSessionStorage.DeleteAsync(StorageKey);

        if (notify)
        {
            NotifyStateChanged();
        }
    }

    private void ClearState(bool notify = true)
    {
        Username = null;
        AccessToken = null;
        ExpiresAtUtc = null;

        if (notify)
        {
            NotifyStateChanged();
        }
    }

    private void NotifyStateChanged()
    {
        StateChanged?.Invoke();
    }

    private static DateTimeOffset? ResolveExpirationUtc(string token)
    {
        try
        {
            var parts = token.Split('.');
            if (parts.Length < 2)
            {
                return null;
            }

            var payload = parts[1]
                .Replace('-', '+')
                .Replace('_', '/');

            switch (payload.Length % 4)
            {
                case 2:
                    payload += "==";
                    break;
                case 3:
                    payload += "=";
                    break;
            }

            var payloadBytes = Convert.FromBase64String(payload);
            using var document = JsonDocument.Parse(payloadBytes);

            if (!document.RootElement.TryGetProperty("exp", out var expElement) || !expElement.TryGetInt64(out var expUnixSeconds))
            {
                return null;
            }

            return DateTimeOffset.FromUnixTimeSeconds(expUnixSeconds);
        }
        catch
        {
            return null;
        }
    }
}

public readonly record struct AuthLoginResult(bool Succeeded, string? ErrorMessage)
{
    public static AuthLoginResult Success() => new(true, null);
    public static AuthLoginResult Failure(string errorMessage) => new(false, errorMessage);
}

internal sealed record PersistedAuthSession(string Username, string AccessToken, DateTimeOffset ExpiresAtUtc);
