using TaskHub.Domain.Entities;

namespace TaskHub.Application.abstraction.Repository.Query;

public interface IUserAuthenticationQueryRepository
{
    Task<User?> FindByNormalizedUsernameAsync(string normalizedUsername, CancellationToken ct);
}
