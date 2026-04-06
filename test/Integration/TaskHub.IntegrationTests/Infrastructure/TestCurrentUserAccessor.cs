using System.Security.Claims;
using System.Threading;
using Microsoft.AspNetCore.Http;
using TaskHub.Application.abstraction;

namespace TaskHub.IntegrationTests.Infrastructure;

public sealed class TestCurrentUserAccessor(IHttpContextAccessor httpContextAccessor) : ICurrentUserAccessor
{
    private readonly AsyncLocal<string?> _overrideUserIdentifier = new();

    public string? UserIdentifier
    {
        get
        {
            if (_overrideUserIdentifier.Value is not null)
            {
                return _overrideUserIdentifier.Value;
            }

            var user = httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            return user.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }

    public Guid? UserId
    {
        get
        {
            var userIdentifier = UserIdentifier;
            return Guid.TryParse(userIdentifier, out var userId)
                ? userId
                : null;
        }
    }

    public IDisposable BeginScope(string? userIdentifier)
    {
        var previousValue = _overrideUserIdentifier.Value;
        _overrideUserIdentifier.Value = userIdentifier;
        return new RestoreScope(() => _overrideUserIdentifier.Value = previousValue);
    }

    private sealed class RestoreScope(Action restore) : IDisposable
    {
        public void Dispose()
        {
            restore();
        }
    }
}
