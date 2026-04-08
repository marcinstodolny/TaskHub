using TaskHub.Domain.Entities;

namespace TaskHub.Application.Abstractions.Repositories.Query;

public interface IUserAuthenticationQueryRepository
{
    Task<User?> FindByNormalizedUsernameAsync(string normalizedUsername, CancellationToken ct);
}
