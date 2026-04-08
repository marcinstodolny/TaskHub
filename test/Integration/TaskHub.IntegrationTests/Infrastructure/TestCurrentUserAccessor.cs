using System.Security.Claims;
using System.Threading;
using Microsoft.AspNetCore.Http;
using TaskHub.Application.Abstractions;

namespace TaskHub.IntegrationTests.Infrastructure;

public sealed class TestCurrentUserAccessor(IHttpContextAccessor httpContextAccessor) : ICurrentUserAccessor
{
    private readonly AsyncLocal<Guid?> _overrideUserId = new();

    public Guid? UserId
    {
        get
        {
            if (_overrideUserId.Value is not null)
            {
                return _overrideUserId.Value;
            }

            var user = httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdClaim, out var userId)
                ? userId
                : null;
        }
    }

    public IDisposable BeginScope(Guid? userId)
    {
        var previousValue = _overrideUserId.Value;
        _overrideUserId.Value = userId;
        return new RestoreScope(() => _overrideUserId.Value = previousValue);
    }

    private sealed class RestoreScope(Action restore) : IDisposable
    {
        public void Dispose()
        {
            restore();
        }
    }
}
