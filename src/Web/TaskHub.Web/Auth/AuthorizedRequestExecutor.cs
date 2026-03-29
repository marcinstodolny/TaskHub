using System.Net;

namespace TaskHub.Web.Auth;

public sealed class AuthorizedRequestExecutor(AuthorizedApiClientFactory apiClientFactory)
{
    public async Task<HttpResponseMessage> SendAsync(
        Func<HttpClient, Task<HttpResponseMessage>> request,
        bool retryOnForbidden = true,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var response = await SendCoreAsync(forceRefreshToken: false, ct);
        if (response.StatusCode != HttpStatusCode.Unauthorized &&
            (!retryOnForbidden || response.StatusCode != HttpStatusCode.Forbidden))
        {
            return response;
        }

        response.Dispose();
        return await SendCoreAsync(forceRefreshToken: true, ct);

        async Task<HttpResponseMessage> SendCoreAsync(bool forceRefreshToken, CancellationToken cancellationToken)
        {
            var client = await apiClientFactory.CreateAsync(forceRefreshToken, cancellationToken);
            return await request(client);
        }
    }
}
