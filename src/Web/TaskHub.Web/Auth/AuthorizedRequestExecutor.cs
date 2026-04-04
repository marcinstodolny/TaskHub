using System.Net;

namespace TaskHub.Web.Auth;

public sealed class AuthorizedRequestExecutor(AuthorizedApiClientFactory apiClientFactory, AuthSession authSession)
{
    public async Task<HttpResponseMessage> SendAsync(
        Func<HttpClient, Task<HttpResponseMessage>> request,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var client = await apiClientFactory.CreateAsync(ct);
        var response = await request(client);

        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            await authSession.LogoutAsync(ct);
        }

        return response;
    }
}
