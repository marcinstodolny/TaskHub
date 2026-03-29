using System.Net.Http.Headers;

namespace TaskHub.Web.Auth;

public sealed class AuthorizedApiClientFactory(IHttpClientFactory httpClientFactory, DemoAccessTokenProvider tokenProvider)
{
    public async Task<HttpClient> CreateAsync(CancellationToken ct = default)
    {
        var accessToken = await tokenProvider.GetAccessTokenAsync(ct);

        var apiClient = httpClientFactory.CreateClient("TaskHubApi");
        apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        return apiClient;
    }
}
