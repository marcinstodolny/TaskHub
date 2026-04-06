using TaskHub.Domain.Entities;

namespace TaskHub.Application.abstraction.Repository.Command
{
    public interface IUserCommandRepository
    {
        Task<bool> ExistsByUsernameAsync(string username, CancellationToken ct);
        Task AddAsync(User user, CancellationToken ct);
    }
}
