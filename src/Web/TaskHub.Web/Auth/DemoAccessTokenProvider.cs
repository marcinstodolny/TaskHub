using System.Text.Json;
using Microsoft.Extensions.Options;
using TaskHub.Contracts.Auth;

namespace TaskHub.Web.Auth;

public sealed class DemoAccessTokenProvider(IHttpClientFactory httpClientFactory, IOptions<DemoAuthOptions> demoAuthOptions)
{
    private string? _accessToken;
    private DateTimeOffset? _accessTokenExpiresAtUtc;

    public async Task<string> GetAccessTokenAsync(CancellationToken ct = default)
    {
        if (HasValidToken())
        {
            return _accessToken!;
        }

        var options = demoAuthOptions.Value;

        var tokenRequest = new TokenRequest(options.Username, options.Password);
        var apiClient = httpClientFactory.CreateClient("TaskHubApi");
        using var response = await apiClient.PostAsJsonAsync("/api/auth/token", tokenRequest, ct);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException("Unable to sign in to demo API.");
        }

        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: ct);

        if (string.IsNullOrWhiteSpace(tokenResponse?.AccessToken))
        {
            throw new InvalidOperationException("Demo API returned an invalid token.");
        }

        _accessToken = tokenResponse.AccessToken;
        _accessTokenExpiresAtUtc = ResolveExpirationUtc(_accessToken);

        return _accessToken;
    }

    private bool HasValidToken()
    {
        if (string.IsNullOrWhiteSpace(_accessToken))
        {
            return false;
        }

        if (_accessTokenExpiresAtUtc is null)
        {
            return true;
        }

        return _accessTokenExpiresAtUtc > DateTimeOffset.UtcNow.AddMinutes(1);
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
