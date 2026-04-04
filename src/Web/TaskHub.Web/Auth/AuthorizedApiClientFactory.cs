using System.Net.Http.Headers;

namespace TaskHub.Web.Auth;

public sealed class AuthorizedApiClientFactory(IHttpClientFactory httpClientFactory, AuthSession authSession)
{
    public async Task<HttpClient> CreateAsync(CancellationToken ct = default)
    {
        var apiClient = httpClientFactory.CreateClient("TaskHubApi");
        apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await authSession.GetRequiredAccessTokenAsync(ct));

        return apiClient;
    }
}
